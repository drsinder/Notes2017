/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: ExportController.cs
**
**  Description:
**      Export Controller for Notes 2017
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2017.Models;
using Notes2017.ViewModels;
using Notes2017.Services;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.ApplicationInsights;
using Notes2017.App_Code;
using Notes2017.Data;
using Microsoft.AspNetCore.Hosting;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "User")]
    public class ExportController : NController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly string _stylePath;
        private readonly TelemetryClient _telemetry;

        public ExportController(
            IHostingEnvironment appEnv,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ApplicationDbContext applicationDbContext, TelemetryClient tel) : base(userManager, signInManager)
        {
            _userManager = userManager;
            _db = applicationDbContext;
            _stylePath = appEnv.ContentRootPath + "\\wwwroot\\css\\ExportCSS\\NotesExport31.css";

            _telemetry = tel;
            _telemetry.InstrumentationKey = Global.InstKey;
        }

        /// <summary>
        /// Gather info from user - what file, what options to use
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            //string userid = _userManager.GetUserId(User);
            IEnumerable<SelectListItem> items = NoteDataManager.GetFileNameSelectList(_db);
            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a File --"
                }
            };
            list2.AddRange(items);

            // create new model and start adding properties
            NFViewModel it = new NFViewModel {AFiles = list2};
            // file name select list
            items = NoteDataManager.GetFileTitleSelectList(_db);
            list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a Title --"
                }
            };
            list2.AddRange(items);
            it.ATitles = list2;

            // set default options
            it.isHtml = true;
            it.isCollapsible = true;
            it.directOutput = false;
            it.NoteOrdinal = 0;

            return View(it);
        }

        /// <summary>
        /// Gets html created on the fly for download
        /// </summary>
        /// <param name="model">contains file name, options</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<FileResult> Index(NFViewModel model)
        {
            return File(await DoExport(model, User), System.Net.Mime.MediaTypeNames.Application.Octet, model.FileName + (model.isHtml ? ".html" : ".txt"));
        }

        public async Task<IActionResult> PreExport(int id)
        {
            //string userid = _userManager.GetUserId(User);
            NFViewModel it = new NFViewModel();

            NoteFile nf = await NoteDataManager.GetFileById(_db, id);
            it.FileName = nf.NoteFileName;
            it.FileNum = id;

            // set default options
            it.isHtml = false;
            it.isCollapsible = false;
            it.directOutput = true;
            it.NoteOrdinal = 0;

            return View(it);
        }

        //public async Task<FileResult> ExportText(int id)
        //{
        //    NFViewModel model = new NFViewModel();
        //    model.isHtml = false;
        //    model.isCollapsible = false;
        //    model.directOutput = false;
        //    model.NoteOrdinal = 0;
        //    NoteFile nf = await NoteDataManager.GetFileById(db, id);
        //    model.FileName = nf.NoteFileName;

        //    return File(await DoExport(model, User), System.Net.Mime.MediaTypeNames.Application.Octet, model.FileName + ".txt");
        //}

        /// <summary>
        /// Does the creation of a note item as html fragment
        /// </summary>
        /// <param name="sw">Memory stream writer</param>
        /// <param name="nc">Note content object</param>
        /// <param name="bnh">Base note header object</param>
        /// <param name="isHtml">html vs plain text</param>
        /// <param name="isResponse">response or base note</param>
        /// <returns></returns>
        public async Task<bool> WriteNote(StreamWriter sw, NoteContent nc, BaseNoteHeader bnh,  bool isHtml, bool isResponse)
        {
            if (isHtml)
            {
                if (!isResponse)
                    await sw.WriteLineAsync("<div class=\"base-note\">");
                else
                    await sw.WriteLineAsync("<div class=\"response-note\">");

                await sw.WriteLineAsync("<h3>");
            }

            if (!isResponse)
            {
                // write base note header
                await sw.WriteLineAsync("Note: " + nc.NoteOrdinal + " - Subject: "
                + nc.NoteSubject + (isHtml ? "<br />" : " - ") + "Author: " + nc.AuthorName + " - "
                + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " - "
                + bnh.Responses + " Response" + (bnh.Responses > 1 || bnh.Responses == 0 ? "s" : ""));
            }
            else
            {
                // write response note header
                await sw.WriteLineAsync("Note: " + nc.NoteOrdinal + " - Subject: "
                + nc.NoteSubject + (isHtml ? "<br />" : " - ") + "Author: " + nc.AuthorName + " - "
                + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " - "
                + "Response " + nc.ResponseOrdinal + " of " + bnh.Responses);
                await sw.WriteLineAsync((isHtml ? "<br />" : string.Empty) + "Base Note Subject: " + bnh.NoteSubject);
            }

            if (isHtml)
                await sw.WriteLineAsync("</h3>");

            if (!isHtml || !string.IsNullOrEmpty(nc.DirectorMesssage))
            {
                await sw.WriteLineAsync((isHtml ? "<h5>" : "") + "Tags - " + nc.DirectorMesssage + (isHtml ? "</h5>" : ""));
            }
            await sw.WriteLineAsync();
            
            if (isHtml)
                await sw.WriteLineAsync(nc.NoteBody.Replace("<br />", "<br />\r\n") + "</div>");
            else
                await sw.WriteLineAsync(nc.NoteBody.Replace("<br />", "\r\n"));
            await sw.WriteLineAsync();

            return true;
        }

        /// <summary>
        /// Create an Html representation of the notefile
        /// </summary>
        /// <param name="model">Notefile and options</param>
        /// <param name="user">User (object) making request</param>
        /// <returns>MemoryStream containing the Html</returns>
        public async Task<MemoryStream> DoExport(NFViewModel model, ClaimsPrincipal user)
        {
            // get our options
            bool isHtml = model.isHtml;
            bool isCollapsible = model.isCollapsible;

            // make sure we have a valid file name
            NoteFile nf = await NoteDataManager.GetFileByName(_db, model.FileName);

            if (nf == null)
                return null;

            int nfid = nf.NoteFileID;

            //string userid = _userManager.GetUserId(user);
            string userName = _userManager.GetUserName(user);
            NoteAccess ac = await GetMyAccess(nfid, user);
            if (!ac.Read)       // make sure user has read access to file
                return null;

            string evt = "Export of file " + model.FileName;
            if (model.NoteOrdinal != 0)
                evt += " note # " + model.NoteOrdinal;
            evt += " as " + (isHtml ? "html" : "txt") + " for " + userName;
            _telemetry.TrackEvent(evt);

            //string filename = model.FileName + (isHtml ? ".html" : ".txt");

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            StringBuilder sb = new StringBuilder();
            if (isHtml)
            {
                // Start the document
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("<meta charset=\"utf-8\" />");
                sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                sb.AppendLine("<title>" + nf.NoteFileTitle + "</title>");
                sb.AppendLine("<link rel = \"stylesheet\" href = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css\">");

                //sb.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\" ></script >");
                //sb.AppendLine("<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js\" ></script >");
                //sb.AppendLine("<script src = \"https://Notes2017.net/js/prism.min.js\" ></script >");

                sb.AppendLine("<style>");

                // read our local style sheet from a file and output it
                TextReader sr = new StreamReader(_stylePath);
                sb.AppendLine(await sr.ReadToEndAsync());
                sr.Close();

                sb.AppendLine("</style>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                await sw.WriteAsync(sb.ToString());

                // ready to start  writing content of file
                sb = new StringBuilder();
            }
            if (isHtml)
                sb.Append("<h2>");

            // File Header
            sb.Append("NoteFile " + nf.NoteFileName + " - " + nf.NoteFileTitle);
            sb.Append(" - Created " + DateTime.Now.ToUniversalTime().ToLongDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
            sb.Append("   For " + userName);
            if (isHtml)
                sb.Append("</h2>");
            await sw.WriteLineAsync(sb.ToString());
            await sw.WriteLineAsync();

            // get ordered list of basenoteheaders to start process
            List<BaseNoteHeader> bnhl;
            if (model.NoteOrdinal == 0)
            {
                bnhl = await NoteDataManager.GetBaseNoteHeadersForFile(_db, nfid);
            }
            else
            {
                bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nfid, model.NoteOrdinal);
            }
            // loop over each base note in order
            foreach (BaseNoteHeader bnh in bnhl)
            {
                // get content for base note
                NoteContent nc = await NoteDataManager.GetNoteById(_db, bnh.NoteID);

                // format it and write it
                await WriteNote(sw, nc, bnh, isHtml, false);

                // get ordered list of responses
                List<NoteContent> rcl = await NoteDataManager.GetOrderedListOfResponses(_db, nfid, bnh);

                await sw.WriteLineAsync();
                // extra stuff for collapsable responses
                if (isCollapsible && isHtml && rcl.Any())
                {
                    await sw.WriteLineAsync("<div class=\"container\"><div class=\"panel-group\">" +
                        "<div class=\"panel panel-default\"><div class=\"panel-heading\"><div class=\"panel-title\"><a data-toggle=\"collapse\" href=\"#collapse" +
                        nc.NoteOrdinal + "\">Toggle " + bnh.Responses + " Response" + (bnh.Responses > 1 ? "s" : "") + "</a></div></div><div id = \"collapse" + nc.NoteOrdinal +
                        "\" class=\"panel-collapse collapse\"><div class=\"panel-body\">");
                }
                // loop over each respponse for a base note
                foreach (NoteContent rc in rcl)
                {
                    await WriteNote(sw, rc, bnh, isHtml, true);
                }
                // extra stuff to terminate collapsable responses
                if (isCollapsible && isHtml && rcl.Any())
                {
                    await sw.WriteLineAsync("</div></div></div></div></div> ");
                }
            }

            if (isHtml)  // end the html
            {
                await sw.WriteLineAsync("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\" ></script >");
                await sw.WriteLineAsync("<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js\" ></script >");
                await sw.WriteLineAsync("<script src = \"https://Notes2017.ddns.net/js/prism.min.js\" ></script >");

                await sw.WriteLineAsync("</body></html>");
            }

            // make sure all output is written to stream and rewind it
            await sw.FlushAsync();
            ms.Seek(0, SeekOrigin.Begin);
            // send stream to caller
            return ms;
        }


        public IActionResult ExportMarked()
        {
            NFViewModel v = new NFViewModel
            {
                directOutput = false,
                isCollapsible = false,
                isHtml = true
            };
            int fid = _db.Mark
                .First(p => p.UserID == _userManager.GetUserId(User))
                .NoteFileID;

            v.FileName = _db.NoteFile
                .First(p => p.NoteFileID == fid)
                .NoteFileName;

            return View(v);
        }

        /// <summary>
        /// Gets html created on the fly for download
        /// </summary>
        /// <param name="model">contains file name, options</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<FileResult> ExportMarked(NFViewModel model)
        {
            return File(await DoExportMarked(model, User), System.Net.Mime.MediaTypeNames.Application.Octet, model.FileName + (model.isHtml ? ".html" : ".txt"));
        }

        /// <summary>
        /// Create an Html representation of the notefile
        /// </summary>
        /// <param name="model">Notefile and options</param>
        /// <param name="user">User (object) making request</param>
        /// <returns>MemoryStream containing the Html</returns>
        public async Task<MemoryStream> DoExportMarked(NFViewModel model, ClaimsPrincipal user)
        {
            // get our options
            bool isHtml = model.isHtml;
            bool isCollapsible = model.isCollapsible;

            // make sure we have a valid file name
            NoteFile nf = await NoteDataManager.GetFileByName(_db, model.FileName);

            if (nf == null)
                return null;

            int nfid = nf.NoteFileID;

            //string userid = _userManager.GetUserId(user);
            string userName = _userManager.GetUserName(user);
            NoteAccess ac = await GetMyAccess(nfid, user);
            if (!ac.Read)       // make sure user has read access to file
                return null;

            //string filename = model.FileName + (isHtml ? ".html" : ".txt");

            string evt = "Export of Marked notes in file " + model.FileName;
            evt += " as " + (isHtml ? "html" : "txt") + " for " + userName;
            _telemetry.TrackEvent(evt);

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            StringBuilder sb = new StringBuilder();
            if (isHtml)
            {
                // Start the document
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("<meta charset=\"utf-8\" />");
                sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                sb.AppendLine("<title>" + nf.NoteFileTitle + "</title>");
                sb.AppendLine("<link rel = \"stylesheet\" href = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css\">");
                if (isCollapsible)
                {
                    sb.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\" ></script >");
                    sb.AppendLine("<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js\" ></script >");
                }
                sb.AppendLine("<style>");

                // read our local style sheet from a file and output it
                TextReader sr = new StreamReader(_stylePath);
                sb.AppendLine(await sr.ReadToEndAsync());
                sr.Close();

                sb.AppendLine("</style>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                await sw.WriteAsync(sb.ToString());

                // ready to start  writing content of file
                sb = new StringBuilder();
            }
            if (isHtml)
                sb.Append("<h2>");

            // File Header
            sb.Append("NoteFile " + nf.NoteFileName + " - " + nf.NoteFileTitle);
            sb.Append(" - Created " + DateTime.Now.ToUniversalTime().ToLongDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
            sb.Append("   For " + userName);
            if (isHtml)
                sb.Append("</h2>");
            await sw.WriteLineAsync(sb.ToString());
            await sw.WriteLineAsync();

            var query = _db.Mark
                .OrderBy(p => p.MarkOrdinal)
                .Where(p => p.UserID == _userManager.GetUserId(user));

            // loop over each note in order
            IEnumerator<Mark> mrk = query.GetEnumerator();
            mrk.MoveNext();
            bool inResp = false;
            int nOrd = mrk.Current.NoteOrdinal;

            while (mrk.Current != null)
            {
                Mark mark = mrk.Current;
                mrk.MoveNext();
                // get content for note
                NoteContent nc = await NoteDataManager.GetMarkedNote(_db, mark);

                BaseNoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, mark.NoteFileID, mark.NoteOrdinal);
                    
                    //db.BaseNoteHeader
                    //.Where(p => p.NoteFileID == mark.FileID && p.NoteOrdinal == mark.NoteOrdinal)
                    //.FirstAsync();

                // extra stuff to terminate collapsable responses
                //                if (isCollapsible && isHtml && inResp && nc.ResponseOrdinal == 0 )  // || (nc.NoteOrdinal != NOrd))
                if (isCollapsible && isHtml && inResp && (nc.NoteOrdinal != nOrd))
                {
                    inResp = false;
                    await sw.WriteLineAsync("</div></div></div></div></div> ");
                }

                if ((isCollapsible && isHtml && nc.ResponseOrdinal > 0) && !inResp)  // && (NOrd != nc.NoteOrdinal))
                {
                    inResp = true;
                    await sw.WriteLineAsync("<div class=\"container\"><div class=\"panel-group\">" +
                        "<div class=\"panel panel-default\"><div class=\"panel-heading\"><div class=\"panel-title\"><a data-toggle=\"collapse\" href=\"#collapse" +
                        nc.NoteOrdinal + "\">Toggle Response(s)" + "</a></div></div><div id = \"collapse" + nc.NoteOrdinal +
                        "\" class=\"panel-collapse collapse\"><div class=\"panel-body\">");
                }

                // format it and write it
                await WriteNote(sw, nc, bnh, isHtml, nc.ResponseOrdinal > 0);
                nOrd = nc.NoteOrdinal;

                await sw.WriteLineAsync();
            }
            mrk.Dispose();

            // extra stuff to terminate collapsable responses
            if (isCollapsible && isHtml && inResp)
            {
                await sw.WriteLineAsync("</div></div></div></div></div> ");
            }

            if (isHtml)  // end the html
                await sw.WriteLineAsync("</body></html>");

            // make sure all output is written to stream and rewind it
            await sw.FlushAsync();
            ms.Seek(0, SeekOrigin.Begin);

            mrk = query.GetEnumerator();
            mrk.MoveNext();
            while (mrk.Current != null)
            {
                _db.Mark.Remove(mrk.Current);
                mrk.MoveNext();
            }
            mrk.Dispose();
            await _db.SaveChangesAsync();
            // send stream to caller
            return ms;
        }



        /// <summary>
        /// Get the access object for this user in this file
        /// </summary>
        /// <param name="fileid">id of file we are exporting</param>
        /// <param name="user">id of this user</param>
        /// <returns></returns>
        public async Task<NoteAccess> GetMyAccess(int fileid, ClaimsPrincipal user)
        {
            NoteAccess noteAccess = await AccessManager.GetAccess(_db, _userManager.GetUserId(user), fileid);
            ViewData["MyAccess"] = noteAccess;
            return noteAccess;
        }

    }
}
