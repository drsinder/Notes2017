/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteDisplayController.cs
**
**  Description:
**      Note Display Controller for Notes 2017
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Notes2017.Models;
using Notes2017.ViewModels;
using Notes2017.Services;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.ApplicationInsights;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
//using Hangfire;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "User")]
    public class NoteDisplayController : NController
    {
        private readonly IHostingEnvironment _appEnv;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ApplicationDbContext _db;
        private readonly TelemetryClient _telemetry;

        public NoteDisplayController(
            IHostingEnvironment appEnv,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ApplicationDbContext applicationDbContext, 
            TelemetryClient tel) : base(userManager, signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _appEnv = appEnv;
            _db = applicationDbContext;

            _telemetry = tel;
            _telemetry.InstrumentationKey = Globals.InstKey;

            Globals.EmailSender = _emailSender;
        }

        /// <summary>
        /// Utility to enter file based on contents of a NFViewModel Object
        /// </summary>
        /// <param name="model">NFViewModel Object</param>
        /// <returns></returns>
        public async Task<IActionResult> EnterFile(NFViewModel model)
        {
            NoteFile nf = await NoteDataManager.GetFileByName(_db, model.FileName);

            if (nf != null)
            {
                int id = nf.NoteFileID;
                if (_signInManager.IsSignedIn(User))
                {
                    ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));
                    HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(user.Pref1));
                }

                return RedirectToAction("Index", new { id });
            }
            ViewBag.Message = "Could not find file '" + model.FileName + "'.";
            return View("Error");
        }


        /// <summary>
        /// Utility to enter a file based on a NoteFileID
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> EnterFileByID(int id)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, id);

            if (nf != null)
            {
                if (_signInManager.IsSignedIn(User))
                {
                    ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));
                    HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(user.Pref1));
                }

                return RedirectToAction("Index", new { id });
            }
            ViewBag.Message = "Could not find fileID '" + id + "'.";
            return View("Error");
        }

        /// <summary>
        /// Utility to enter a file based on it's name
        /// </summary>
        /// <param name="id">NoteFileName</param>
        /// <returns></returns>
        public async Task<IActionResult> Enter(string id)
        {
            NoteFile noteFile = await NoteDataManager.GetFileByName(_db, id);

            if (noteFile != null)
            {
                if (_signInManager.IsSignedIn(User))
                {
                    ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));
                    HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(user.Pref1));
                }
                return RedirectToAction("Index", new { id = noteFile.NoteFileID });
            }
            ViewBag.Message = "Could not find file '" + id + "'.";
            return View("Error");

        }

        /// <summary>
        /// Prepare data for display of NoteFile Index or List of Base Notes
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? id)
        {
            HttpContext.Session.SetInt32("IsSearch", 0);  // Mark not doing a search

            bool isAdmin = User.IsInRole("Admin");

            if (id == null)
            {
                return RedirectToAction("Index", "NoteFileList");
                //ViewBag.Message = "FileID given is null.";
                //return View("Error");
            }

            // Check that user can read and/or write the file
            NoteAccess myacc = await GetMyAccess((int)id);
            if (myacc == null)
            {
                ViewBag.Message = "Could not find access for fileid '" + id + "'.";
                return View("Error");
            }
            if (!myacc.Read && !myacc.Write)
            {
                NoteFile nf = await NoteDataManager.GetFileById(_db, (int)id);
                if (nf == null)
                {
                    ViewBag.Message = "Could not find fileid '" + id + "'.";
                    return View("Error");
                }
                ViewBag.Message = "You do not have access to file '" + nf.NoteFileName + "'.";
                return View("Error");
            }

            // Get the NoteFile Object
            NoteFile noteFile = await NoteDataManager.GetFileByIdWithHeaders(_db, (int)id);

            _telemetry.TrackEvent("Index of " + noteFile.NoteFileName);

            if (noteFile == null)
            {
                ViewBag.Message = "Could not find fileid '" + id + "'.";
                return View("Error");
            }

            if (isAdmin)
            {
                myacc.Director = true;
            }
            NoteDisplayIndexModel idxModel = new NoteDisplayIndexModel()
            {
                myAccess = myacc,
                noteFile = noteFile,
                ExpandOrdinal = 0,

                tZone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db)
            };
            Mark mark = await _db.Mark.Where(p => p.UserID == _userManager.GetUserId(User)).FirstOrDefaultAsync();
            idxModel.isMarked = (mark != null);

            idxModel.rPath = Request.PathBase;

            //byte[] b2 = HttpContext.Session.Get("MyStyle");
            //if (b2 != null)
            //    ViewBag.MyStyle = Encoding.Default.GetString(b2);

            // Pass NoteFile list to View
            return View(_userManager, idxModel);
        }

        public async Task<IActionResult> Expand(long? id)
        {
            HttpContext.Session.SetInt32("IsSearch", 0);  // Mark not doing a search

            if (id == null)
            {
                ViewBag.Message = "Base NoteID given is null.";
                return View("Error");
            }

            NoteContent note = await NoteDataManager.GetNoteById(_db, (long)id);
            if (note == null)
            {
                ViewBag.Message = "Could not find note for Base NoteID '" + id + "'.";
                return View("Error");
            }


            // Check that user can read and/or write the file
            NoteAccess myacc = await GetMyAccess(note.NoteFileID);
            if (myacc == null)
            {
                ViewBag.Message = "Could not find access for fileid '" + id + "'.";
                return View("Error");
            }
            else if (!myacc.Read && !myacc.Write)
            {
                NoteFile nf = await NoteDataManager.GetFileById(_db, (int)id);
                if (nf == null)
                {
                    ViewBag.Message = "Could not find fileid '" + id + "'.";
                    return View("Error");
                }
                ViewBag.Message = "You do not have access to file '" + nf.NoteFileName + "'.";
                return View("Error");
            }

            // Get the NoteFile Object
            NoteFile noteFile = await NoteDataManager.GetFileByIdWithHeaders(_db, note.NoteFileID);

            if (noteFile == null)
            {
                ViewBag.Message = "Could not find fileid '" + id + "'.";
                return View("Error");
            }

            _telemetry.TrackEvent("Expanded Index of " + noteFile.NoteFileName + " Note " + note.NoteOrdinal);

            NoteDisplayIndexModel idxModel = new NoteDisplayIndexModel()
            {
                myAccess = myacc,
                noteFile = noteFile,
                ExpandOrdinal = note.NoteOrdinal,
                Notes = await NoteDataManager.GetBaseNoteAndResponses(_db, note.NoteFileID, note.NoteOrdinal)
            };
            if (idxModel.Notes == null)
            {
                ViewBag.Message = "Could not get base note '" + note.NoteOrdinal + "' and responses for '" + noteFile.NoteFileName + "'.";
                return View("Error");
            }

            idxModel.Cheaders = new List<string>();
            idxModel.Lheaders = new List<string>();

            foreach ( NoteContent nc in idxModel.Notes)
            {
                idxModel.Cheaders.Add("<div class=\"container\"><div class=\"panel-group\">"
                        + "<div class=\"panel panel-default\"><div class=\"panel-heading\"><div class=\"panel-title\">"
                        + "</div></div><div id = \"collapse" + nc.NoteID
                        + "\" class=\"panel-collapse collapse\"><div class=\"panel-body\">");

                idxModel.Lheaders.Add("<a data-toggle =\"collapse\" href=\"#collapse" + nc.NoteID + "\">&gt;</a>");
            }

            idxModel.panelEnd = "</div></div></div></div></div>";

            Mark mark = await _db.Mark.Where(p => p.UserID == _userManager.GetUserId(User)).FirstOrDefaultAsync();
            idxModel.isMarked = (mark != null);

            idxModel.rPath = Request.PathBase;

            idxModel.tZone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            // Pass NoteFile list to View
            return View(_userManager, "Index", idxModel);
        }

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        /// <summary>
        /// Display a note and present list of actions
        /// </summary>
        /// <param name="id">NoteID of Note to display</param>
        /// <returns></returns>
        public async Task<IActionResult> Display(long? id)
        {
            if (id == null)
            {
                //return RedirectToAction("Index", "NoteFileList");
                ViewBag.Message = "NoteID given is null.";
                return View("Error");
            }

            // Get the Note Object
            NoteContent nc = await NoteDataManager.GetNoteByIdWithFile(_db, (long)id);
            if (nc == null)
            {
                ViewBag.Message = "NoteID '" + (long)id + "' could not be found.";
                return View("Error");
            }

            string page = "Display " + nc.NoteFile.NoteFileName + " Note " + nc.NoteOrdinal;
            if (nc.ResponseOrdinal > 0)
                page += "." + nc.ResponseOrdinal;
            _telemetry.TrackEvent(page);


            NoteXModel model = await GetNoteExtras(nc);  // set some Model Data
            model.myAccess = await GetMyAccess(nc.NoteFileID);

            // Check if sequencing
            if (HttpContext.Session.GetInt32("CurrentSeq") != null && HttpContext.Session.GetInt32("CurrentSeq") == 1)
                model.IsSeq = true;
            else
                model.IsSeq = false;

            model.tZone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            int? menu = HttpContext.Session.GetInt32("HideNoteMenu");
            ViewBag.HideNoteMenu = false;
            if(menu != null && menu > 0)
                ViewBag.HideNoteMenu = true;

            //byte[] b2 = HttpContext.Session.Get("MyStyle");
            //if (b2 != null)
            //    ViewBag.MyStyle = Encoding.Default.GetString(b2);

            // Send NoteXModel Object to View
            return View(_userManager, model);
        }

        /// <summary>
        /// Set a variety of ViewData items for user by the View
        /// </summary>
        /// <param name="nc"></param>
        public async Task<NoteXModel> GetNoteExtras(NoteContent nc)
        {
            NoteXModel model = new NoteXModel()
            {
                note = nc,
                bnh = await GetBaseNoteHeader(nc),

                CanDelete = true,
                DeleteMessage = ""
            };
            if (model.bnh.Responses > 0 && nc.ResponseOrdinal == 0)   // base note with responses
            {
                model.CanDelete = false;
                model.DeleteMessage = "You may not delete/edit this Base Note because it has response(s).";
            }
            else if (model.bnh.Responses > 0 && nc.ResponseOrdinal < model.bnh.Responses)  // not the last response
            {
                model.CanDelete = false;
                model.DeleteMessage = "You may not delete/edit this Response because Response(s) follow it.";
            }

            // now allow delete only by note writer
            if  (model.CanDelete)
            {
                if(string.Compare(_userManager.GetUserId(User), nc.AuthorID) != 0)
                {
                    model.CanDelete = false;
                    model.DeleteMessage = "You are not the writer of this Note.";
                }
            }

            if (User.IsInRole("Admin"))
            { model.CanDelete = true; }

            return model;
        }

        /// <summary>
        /// Get a BaseNoteHeader Object given a NoteContent Object
        /// </summary>
        /// <param name="nc">NoteContent Object</param>
        /// <returns></returns>
        public async Task<BaseNoteHeader> GetBaseNoteHeader(NoteContent nc)
        {
            return await NoteDataManager.GetBaseNoteHeader(_db, nc.NoteFileID, nc.NoteOrdinal);
        }

        /// <summary>
        /// Goto Base Note Next or Previous
        /// </summary>
        /// <param name="id">NoteID of current Note</param>
        /// <param name="inc">1 or -1 for going forward 1 or backward 1</param>
        /// <returns></returns>
        public async Task<long?> GotoBase(long? id, int inc)
        {
            if (id == null)
            {
                return null;
            }
            NoteContent nc = await NoteDataManager.GetNoteById(_db, (long)id);
            if (nc == null)
            {
                return null;
            }
            long? nextId = await FindBaseNoteID(nc.NoteFileID, nc.NoteOrdinal + inc);
            return nextId;
        }
        /// <summary>
        /// Goto Next Base Note
        /// </summary>
        /// <param name="id">NoteID of current Note</param>
        /// <returns></returns>
        public async Task<IActionResult> NextBase(long? id)
        {
            long? newid = await GotoBase(id, 1);
            if (newid == null)
            {
                NoteContent nc = await NoteDataManager.GetNoteById(_db, (long)id);
                return RedirectToAction("Index", new { id = nc.NoteFileID });
            }
            return RedirectToAction("Display", new { id = newid });
        }

        /// <summary>
        /// Goto previous Base Note
        /// </summary>
        /// <param name="id">NoteID of current Note</param>
        /// <returns></returns>
        public async Task<IActionResult> PrevBase(long? id)
        {
            return RedirectToAction("Display", new { id = await GotoBase(id, -1) });
        }

        /// <summary>
        /// Goto Next or Previous Response or Note
        /// </summary>
        /// <param name="id">Current NoteID</param>
        /// <param name="inc">1 or -1 for forawrd or backward</param>
        /// <returns></returns>
        public async Task<long?> Goto(long? id , int inc)
        {
            if (id == null)
            {
                return null;
            }
            NoteContent nc = await NoteDataManager.GetNoteById(_db, (long)id);
            if (nc == null)
            {
                return null;
            }
            var nextId = await NoteDataManager.FindResponseId(_db, nc, nc.ResponseOrdinal + inc) 
                           ?? await FindBaseNoteID(nc.NoteFileID, nc.NoteOrdinal + inc);
            return nextId;
        }

        /// <summary>
        /// Goto Next Resonse or Base Note
        /// </summary>
        /// <param name="id">Current NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> NextNote(long? id)
        {
            long? newid = await Goto(id, 1);
            if ( newid == null)
            {
                NoteContent nc = await NoteDataManager.GetNoteById(_db, (long)id);
                return RedirectToAction("Index", new { id = nc.NoteFileID });
            }
            return RedirectToAction("Display", new { id = newid });
        }

        /// <summary>
        /// Goto Previous Resonse or Base Note
        /// </summary>
        /// <param name="id">Current NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> PrevNote(long? id)
        {
            return RedirectToAction("Display", new { id = await Goto(id, -1) });
        }

        public async Task<IActionResult> Forward(long? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            NoteContent nc = await NoteDataManager.GetNoteById(_db, (long)id);

            ForwardView model = new ForwardView()
            {
                NoteID = nc.NoteID,
                NoteSubject = nc.NoteSubject,
                wholestring = false
            };
            List<BaseNoteHeader> bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nc.NoteFileID, nc.NoteOrdinal);
            BaseNoteHeader bnh = bnhl[0];
            model.hasstring = bnh.Responses > 0;

            NoteAccess myacc = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), nc.NoteFileID);
            if (myacc == null || !myacc.Read)
            {
                ViewBag.Message = "You cannot read file " + nc.NoteFile.NoteFileName;
                return View("Error");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forward(ForwardView fv)
        {
            if (fv == null)
            {
                return BadRequest();
            }

            NoteContent nc = await NoteDataManager.GetNoteByIdWithFile(_db, fv.NoteID);
            ApplicationUser user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            string userEmail = user.Email;
            string userName = user.UserName;

            _telemetry.TrackEvent("Forward note(s) from file " + nc.NoteFile.NoteFileName + " by " + User.Identity.Name);

            await NoteDataManager.SendNotesAsync(fv, _db, _emailSender, userEmail, userName);


            //if (!fv.hasstring || !fv.wholestring)
            //{
            //    BackgroundJob.Enqueue(() => NoteDataManager.SendNote(fv, userEmail, userName, nc));
            //}
            //else
            //{
            //    await NoteDataManager.SendNotesAsync(fv, db, _emailSender, userEmail, userName);
            //}

            return RedirectToAction("Display", new { id = fv.NoteID });
        }


        public async Task<IActionResult> Copy(long? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            NoteContent nc = await NoteDataManager.GetNoteById(_db, (long)id);

            CopyView model = new CopyView()
            {
                NoteID = nc.NoteID,
                NoteSubject = nc.NoteSubject,
                Wholestring = false
            };
            List<BaseNoteHeader> bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nc.NoteFileID, nc.NoteOrdinal);
            BaseNoteHeader bnh = bnhl[0];
            model.Hasstring = bnh.Responses > 0;

            NoteAccess myacc = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), nc.NoteFileID);
            if ( myacc == null || !myacc.Read)
            {
                ViewBag.Message = "You cannot read file " + nc.NoteFile.NoteFileName;
                return View("Error");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Copy(CopyView cv)
        {
            if (cv == null)
            {
                return BadRequest();
            }

            NoteFile nf2 = await NoteDataManager.GetFileByName(_db, cv.ToFile);
            if (nf2 == null)
            {
                ViewBag.Message = "Copy file does not exist: " + cv.ToFile;
                return View("Error");
            }

            NoteAccess myacc = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), nf2.NoteFileID);
            if (myacc == null || !myacc.Write)
            {
                ViewBag.Message = "You cannot write file " + nf2.NoteFileName;
                return View("Error");
            }

            NoteContent nc = await NoteDataManager.GetNoteByIdWithFile(_db, cv.NoteID);
            //ApplicationUser user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            _telemetry.TrackEvent("Copy note(s) from file " + nc.NoteFile.NoteFileName + " to file " + cv.ToFile +" by " + User.Identity.Name);

            if (!cv.Hasstring || !cv.Wholestring)
            {
                NoteContent ncc = new NoteContent()
                {
                    LastEdited = DateTime.Now.ToUniversalTime(),
                    NoteBody = "<p><strong>** =" + nc.NoteFile.NoteFileName + " - " + nc.AuthorName + " - " + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UCT" + " **</strong></p>"
                };
                ncc.NoteBody +=  nc.NoteBody;
                ncc.NoteFileID = nf2.NoteFileID;
                ncc.AuthorName = _userManager.GetUserName(User);
                ncc.AuthorID = _userManager.GetUserId(User);

                ncc.NoteSubject = nc.NoteSubject;
                ncc.DirectorMesssage = nc.DirectorMesssage;
                ncc.ResponseOrdinal = 0;

                await NoteDataManager.CreateNote(_db, _userManager, ncc, true);
            }
            else
            {
                List<BaseNoteHeader> bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nc.NoteFileID, nc.NoteOrdinal);
                BaseNoteHeader bnh = bnhl[0];
                cv.NoteSubject = bnh.NoteSubject;
                List<NoteContent> notes = await NoteDataManager.GetBaseNoteAndResponses(_db, nc.NoteFileID, nc.NoteOrdinal);

                BaseNoteHeader nbnh = null;

                for (int i = 0; i < notes.Count; i++)
                {
                    if (i == 0)
                    {
                        NoteContent ncc = new NoteContent()
                        {
                            LastEdited = DateTime.Now.ToUniversalTime(),
                            NoteBody = "<p><strong>** =" + notes[0].NoteFile.NoteFileName + " - " + notes[0].AuthorName + " - "
                            + notes[0].LastEdited.ToShortDateString() + " " + notes[0].LastEdited.ToShortTimeString() + " UCT" + " **</strong></p>"
                        };
                        ncc.NoteBody += notes[0].NoteBody;
                        ncc.NoteFileID = nf2.NoteFileID;
                        ncc.AuthorName = _userManager.GetUserName(User);
                        ncc.AuthorID = _userManager.GetUserId(User);

                        ncc.NoteSubject = notes[0].NoteSubject;
                        ncc.DirectorMesssage = notes[0].DirectorMesssage;
                        ncc.ResponseOrdinal = 0;

                        var baseNote = await NoteDataManager.CreateNote(_db, _userManager, ncc, true);
                        var nbnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, baseNote.NoteFileID, baseNote.NoteOrdinal);
                        nbnh = nbnhl[0];
                    }
                    else
                    {
                        NoteContent ncc = new NoteContent()
                        {
                            LastEdited = DateTime.Now.ToUniversalTime(),
                            NoteBody = "<p><strong>** =" + notes[i].NoteFile.NoteFileName + " - " + notes[i].AuthorName + " - "
                            + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UCT" + " **</strong></p>"
                        };
                        ncc.NoteBody += notes[i].NoteBody;
                        ncc.NoteFileID = nf2.NoteFileID;
                        ncc.AuthorName = _userManager.GetUserName(User);
                        ncc.AuthorID = _userManager.GetUserId(User);

                        ncc.NoteSubject = notes[i].NoteSubject;
                        ncc.DirectorMesssage = notes[i].DirectorMesssage;
                        ncc.ResponseOrdinal = notes[i].ResponseOrdinal;

                        await NoteDataManager.CreateResponse(_db, _userManager, nbnh, ncc, true);
                    }
                }
            }

            return RedirectToAction("Display", new { id = cv.NoteID });
        }


        /// <summary>
        /// Given a FileID and NoteOrdinal get the Base Note NoteID
        /// </summary>
        /// <returns></returns>
        public async Task<long?> FindBaseNoteID(int fileid, int noteord)
        {
            BaseNoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, fileid, noteord);

            if (bnh == null)
                return null;

            return bnh.NoteID;
        }

        // Use Fancy WYSISYG Html editor
        public void GetIsTinymce()
        {
            ViewData["isTinymce"] = true;
        }

        // GET: NoteDisplay/Create
        /// <summary>
        /// Set up for Create a new Base Note in a given FileID
        /// </summary>
        /// <param name="id">FileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Create(int id)
        {
            GetIsTinymce();

            NoteAccess myAccess = await GetMyAccess(id);
            if(!myAccess.Write)
                return RedirectToAction("Index", "NoteFileList");

            TextViewModel test = new TextViewModel()
            {
                NoteFileID = id,
                BaseNoteHeaderID = 0
            };
            NoteFile nf = await NoteDataManager.GetFileById(_db, id);

            test.noteFile = nf;
            return View(test);
        }

        // GET: NoteDisplay/Create
        /// <summary>
        /// Set up for Create a new Response Note given NoteID
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> CreateResponse(long id)
        {
            GetIsTinymce();

            TextViewModel test = new TextViewModel()
            {
                BaseNoteHeaderID = id
            };
            BaseNoteHeader bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, id);

            test.NoteFileID = bnh.NoteFileID;
            NoteFile nf = await NoteDataManager.GetFileById(_db, bnh.NoteFileID);

            test.noteFile = nf;

            NoteAccess myAccess = await GetMyAccess(bnh.NoteFileID);
            if (!myAccess.Write && !myAccess.Respond)
                return RedirectToAction("Index", "NoteFileList");

            return View("Create", test);
        }



        // POST: NoteDisplay/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Process Creation of new Base and Response Notes
        /// </summary>
        /// <param name="model">NoteContent from View</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        public async Task<IActionResult> Create(TextViewModel model)
        {
            if (!ModelState.IsValid)
            {
                GetIsTinymce();
                return View("Create", model);
            }

            NoteContent nc = new NoteContent()
            {
                LastEdited = DateTime.Now.ToUniversalTime(),
                NoteBody = model.MyNote,
                NoteFileID = model.NoteFileID,
                AuthorName = _userManager.GetUserName(User),
                AuthorID = _userManager.GetUserId(User),

                NoteSubject = model.MySubject,
                DirectorMesssage = model.TagLine,
                ResponseOrdinal = 0
            };
            NoteContent newNote;
            if (model.BaseNoteHeaderID == 0)
                newNote = await NoteDataManager.CreateNote(_db, _userManager, nc, true);
            else
            {
                BaseNoteHeader bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, model.BaseNoteHeaderID);
                    
                newNote = await NoteDataManager.CreateResponse(_db, _userManager, bnh, nc, true);
            }

            return RedirectToAction("Display", new { id = newNote.NoteID });
        }

        // GET: NoteDisplay/Edit/5
        /// <summary>
        /// Prepare to edit a note
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NoteContent note = await NoteDataManager.GetNoteByIdWithFile(_db, (long)id);

            if (note == null)
            {
                return NotFound();
            }

            GetIsTinymce();

            NoteXModel model = await GetNoteExtras(note);
            model.myAccess = await GetMyAccess(note.NoteFileID);

            if (!model.CanDelete)
            {
                ViewData["Note"] = note;
                return RedirectToAction("NoDelete", new {id });
            }

            ViewData["NoteFile"] = note.NoteFile;

            return View(note);
        }

        // POST: NoteDisplay/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Proess edit of a note
        /// </summary>
        /// <param name="nc">NoteContent from View</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        public async Task<IActionResult> Edit(NoteContent nc)
        {
            // Get copy of NoteContent from DB
            NoteContent edited = await NoteDataManager.GetNoteById(_db, nc.NoteID);

            if (edited == null)
            {
                return NotFound();
            }

            // Copy edited values into place
            edited.LastEdited = DateTime.Now.ToUniversalTime();
            edited.NoteBody = nc.NoteBody;
            edited.NoteSubject = nc.NoteSubject;
            edited.DirectorMesssage = nc.DirectorMesssage;
            _db.Entry(edited).State = EntityState.Modified;

            NoteFile nf = await NoteDataManager.GetFileById(_db, edited.NoteFileID);
            // Set file edit datetime
            nf.LastEdited = edited.LastEdited;
            _db.Entry(nf).State = EntityState.Modified;

            // set base note edit datetime
            BaseNoteHeader bnh = await NoteDataManager.GetEditedNoteHeader(_db, edited);
            bnh.LastEdited = edited.LastEdited;
            _db.Entry(bnh).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return RedirectToAction("Display", new { id = edited.NoteID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TypedInputIndex(string typedInput)
        {
            ViewBag.Message = "Not a valid navigation spec: " + typedInput;

            string sfileId = Request.Form["fileID"];
            int fileId = int.Parse(sfileId);
            int noteOrd = 1;
            int respOrd = 0;
            NoteContent nc;
            if (string.IsNullOrEmpty(typedInput) || string.IsNullOrWhiteSpace(typedInput))
                return RedirectToAction("Index", new { id = fileId });

            _telemetry.TrackEvent("TypedInputIndex by " + User.Identity.Name + " for FileID " + fileId + ": '" + typedInput + "'");

            if (typedInput.Contains("."))
            {
                string[] splits = typedInput.Split(new char[] { '.' });
                if (splits.Count() != 2)
                {
                    return View("Error");
                }
                bool ax = !int.TryParse(splits[0], out noteOrd);
                bool bx = !int.TryParse(splits[1], out respOrd);
                if (ax || bx)
                {
                    return View("Error");
                }
                nc = await _db.NoteContent
                    .Where(a => a.NoteFileID == fileId && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == respOrd)
                    .FirstOrDefaultAsync();
            }
            else {
                if (!int.TryParse(typedInput, out noteOrd))
                {
                    return View("Error");
                }
                nc = await _db.NoteContent
                    .Where(a => a.NoteFileID == fileId && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == 0)
                    .FirstOrDefaultAsync();
            }

            if ( nc == null)
            {
                ViewBag.Message = "Note not found: " + typedInput;
                return View("Error");

            }

            return RedirectToAction("Display", new { id = nc.NoteID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TypedInputDisplay(string typedInput)
        {
            ViewBag.Message = "Not a valid navigation spec: " + typedInput;

            string sfileId = Request.Form["fileID"];
            string sOrd = Request.Form["noteord"];
            string sResp = Request.Form["respord"];
            string noteid = Request.Form["noteid"];
            int fileId = int.Parse(sfileId);
            int iOrd = int.Parse(sOrd);
            int iResp = int.Parse(sResp);
            long iNoteId = long.Parse(noteid);
            int noteOrd = 1;
            int respOrd = 0;
            NoteContent nc;
            bool ax = false;
            bool plus = false;
            bool minus = false;

            if (string.IsNullOrEmpty(typedInput) || string.IsNullOrWhiteSpace(typedInput))
                return RedirectToAction("NextNote", new { id = iNoteId });

            _telemetry.TrackEvent("TypedInputDisplay by " + User.Identity.Name + " for FileID " + fileId + ": '" + typedInput + "'");


            if (typedInput.StartsWith("+"))
                plus = true;
            if (typedInput.StartsWith("-"))
                minus = true;
            typedInput = typedInput.Replace("+", "").Replace("-", "");

            if (typedInput.Contains("."))
            {
                string[] splits = typedInput.Split(new char[] { '.' });
                if (splits.Count() != 2)
                {
                    return View("Error");
                }
                if (string.IsNullOrEmpty(splits[0]) || string.IsNullOrWhiteSpace(splits[0]))
                    noteOrd = iOrd;
                else
                    ax = !int.TryParse(splits[0], out noteOrd);
                bool bx = !int.TryParse(splits[1], out respOrd);
                if (ax || bx)
                {
                    return View("Error");
                }

                if (noteOrd == iOrd  && (plus || minus))
                {
                    if (plus)
                        respOrd += iResp;
                    else
                        respOrd = iResp - respOrd;

                    if (respOrd < 0)
                        respOrd = 0;
                    BaseNoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, fileId, noteOrd);

                    if (respOrd > bnh.Responses) respOrd = bnh.Responses;
                }

                nc = await _db.NoteContent
                    .Where(a => a.NoteFileID == fileId && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == respOrd)
                    .FirstOrDefaultAsync();
            }
            else {
                if (!int.TryParse(typedInput, out noteOrd))
                {
                    return View("Error");
                }

                if (!plus && !minus && (noteOrd == 0))
                {
                    return RedirectToAction("Index", new { id = fileId });
                }
                if (plus)
                    noteOrd += iOrd;
                else if (minus)
                    noteOrd = iOrd - noteOrd;

                if (noteOrd < 1) noteOrd = 1;

                NoteFile z = await NoteDataManager.GetFileByIdWithHeaders(_db, fileId);
                List<BaseNoteHeader> bnhl = z.BaseNoteHeaders.ToList();

                long cnt = bnhl.LongCount();
                    
                if (noteOrd > cnt) noteOrd = (int)cnt;

                nc = await _db.NoteContent
                    .Where(a => a.NoteFileID == fileId && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == 0)
                    .FirstOrDefaultAsync();
            }

            if (nc == null)
            {
                ViewBag.Message = "Note not found: " + typedInput;
                return View("Error");
            }

            return RedirectToAction("Display", new { id = nc.NoteID });
        }

        // 
        // GET: NoteDisplay/Delete/5
        // Set up to Delete a base note and all responses
        /// <summary>
        /// Deletes a base note and all responses
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            NoteContent note = await NoteDataManager.GetNoteById(_db, (long)id);

            if (note == null)
            {
                return NotFound();
            }

            NoteXModel model = await GetNoteExtras(note);

            model.myAccess = await GetMyAccess(note.NoteFileID);

            if (!model.CanDelete)
            {
                ViewData["Note"] = note;
                return RedirectToAction("NoDelete", new {id });
            } 
            return View(note);
        }

        /// <summary>
        /// Inform user note can not be deleted
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> NoDelete(long id)
        {
            NoteContent note = await NoteDataManager.GetNoteById(_db, id);

            if (note == null)
            {
                return NotFound();
            }

            NoteXModel model = await GetNoteExtras(note);

            return View(model);
        }

        // POST: NoteDisplay/Delete/5
        /// <summary>
        /// Presess a Note Deletion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            //_userManager.GetUserId(User);
            NoteContent nc = await NoteDataManager.GetNoteById(_db, id);
            await NoteDataManager.DeleteNote(_db, nc);
            return RedirectToAction("Index", new  { id = nc.NoteFileID });
        }

        /// <summary>
        /// Get Access Control Object for file and user
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public async Task<NoteAccess> GetMyAccess(int fileid)
        {
            NoteAccess noteAccess = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), fileid);
            ViewData["MyAccess"] = noteAccess;
            return noteAccess;
        }

        // Searching code begins here

        /// <summary>
        /// Start for a search that begins from Index
        /// </summary>
        /// <param name="id">FileID</param>
        /// <returns></returns>
        public async Task<IActionResult> SearchFromIndex(int id)
        {
            long? firstNoteId = await FindBaseNoteID(id , 1);
            return RedirectToAction("Search", new { id = firstNoteId});
        }

        /// <summary>
        /// Start a search from a note
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> Search(long id)
        {
            NoteContent nc = await NoteDataManager.GetNoteById(_db, id);

            TZone tzone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            //Setup a Search Object with defaults
            Search search = new Search()
            {
                BaseOrdinal = nc.NoteOrdinal,
                NoteFileID = nc.NoteFileID,
                NoteID = nc.NoteID,
                Option = Models.SearchOption.Content,
                ResponseOrdinal = nc.ResponseOrdinal,
                Time = tzone.Local(DateTime.Now.ToUniversalTime()),
                UserID = _userManager.GetUserId(User)
            };
            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "-1",
                    Text = "-- Select a Search Option --"
                },
                new SelectListItem
                {
                    Value = "0",
                    Text = "Author"
                },
                new SelectListItem
                {
                    Value = "1",
                    Text = "Title"
                },
                new SelectListItem
                {
                    Value = "2",
                    Text = "Content"
                },
                new SelectListItem
                {
                    Value = "3",
                    Text = "Tag"
                },
                new SelectListItem
                {
                    Value = "4",
                    Text = "Time After"
                },
                new SelectListItem
                {
                    Value = "5",
                    Text = "Time Before"
                }
            };
            ViewBag.OptionList = list2;

            // Send Search Object to View to Gather user input for search
            return View(search);          
        }

        /// <summary>
        /// Execute search
        /// </summary>
        /// <param name="search">Search Object</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(Search search)
        {
            search.UserID = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                TZone tzone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

                // Perform a search
                Search newSpecs = await DoSearch(search, tzone);

                // Get saved previous Specs
                string userid = _userManager.GetUserId(User);
                Search mysearch = await NoteDataManager.GetUserSearch(_db, userid);

                if (mysearch != null)  // Remove old specs
                {
                    _db.Search.Remove(mysearch);
                    await _db.SaveChangesAsync();
                }

                if (newSpecs.NoteID == search.NoteID || newSpecs.NoteID == 0)
                {
                    HttpContext.Session.SetInt32("IsSearch", 0);
                    return RedirectToAction("Display", new { id = search.NoteID });   // nothing new found   -- temp behavior??
                }

                // Save new specs
                _db.Search.Add(newSpecs);
                await _db.SaveChangesAsync();

                HttpContext.Session.SetInt32("IsSearch", 1);

                // View Note found in Search
                return RedirectToAction("Display", new { id = newSpecs.NoteID });
            }
            return View(search);
        }

        /// <summary>
        /// Continue a search
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ContinueSearch()
        {
            // Get saved search specs
            string userid = _userManager.GetUserId(User);
            Search search = await NoteDataManager.GetUserSearch(_db, userid);

            if (search != null)  // remove specs
            {
                _db.Search.Remove(search);
                await _db.SaveChangesAsync();
            }

            TZone tzone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            // perform continued search
            Search newSpecs = await DoSearch(search, tzone);
            if (search != null && (newSpecs.NoteID == search.NoteID || newSpecs.NoteID == 0))
            {
                HttpContext.Session.SetInt32("IsSearch", 0);
                return RedirectToAction("Display", new { id = search.NoteID });   // nothing new found   -- temp behavior
            }

            // save new specs
            _db.Search.Add(newSpecs);
            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("IsSearch", 1);

            // display note matching search
            return RedirectToAction("Display", new { id = newSpecs.NoteID });
        }

        /// <summary>
        /// Test a note for a match to search specs
        /// </summary>
        /// <param name="nc">NoteContent</param>
        /// <param name="sv">Search</param>
        /// <param name="tzone">TimeZone</param>
        /// <returns>Search for found note</returns>
        private Search SearchTest(NoteContent nc, Search sv, TZone tzone)
        {
            // we can search in a number of ways.  check the ONE the user selected
            switch (sv.Option)
            {
                case Models.SearchOption.Author:

                    if (nc.AuthorName.ToLower().Contains(sv.Text.ToLower()))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.NoteID;
                        return x;
                    }
                    return sv;

                case Models.SearchOption.Content:
                    if (nc.NoteBody.ToLower().Contains(sv.Text.ToLower()))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.NoteID;
                        return x;
                    }
                    return sv;

                case Models.SearchOption.Tag:
                    if (nc.NoteBody.ToLower().Contains(sv.Text.ToLower()))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.NoteID;
                        return x;
                    }
                    return sv;

                case Models.SearchOption.Title:
                    if (nc.NoteSubject.ToLower().Contains(sv.Text.ToLower()))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.NoteID;
                        return x;
                    }
                    return sv;

                case Models.SearchOption.TimeAfter:
                    if (tzone.Local(nc.LastEdited) >= sv.Time)
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.NoteID;
                        return x;
                    }
                    return sv;

                case Models.SearchOption.TimeBefore:
                    if (tzone.Local(nc.LastEdited) <= sv.Time)
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.NoteID;
                        return x;
                    }
                    return sv;

            }
            return sv;
        }

        /// <summary>
        /// Performs a search give a Search that contains specs for search
        /// where we are in the search
        /// </summary>
        /// <param name="start">Search</param>
        /// <param name="tzone">TimeZone</param>
        /// <returns>Search for search match</returns>
        private async Task<Search> DoSearch(Search start, TZone tzone)
        {
            var item0 = await NoteDataManager.GetNoteById(_db, start.NoteID);
            int myRespOrdinal = item0.ResponseOrdinal;
            BaseNoteHeader bnh = await GetBaseNoteHeader(item0);

        // check for reponses beyond current base/resp

        Repeat:
            List<NoteContent> snc = await NoteDataManager.GetSearchResponseList(_db, start, myRespOrdinal, bnh);

            if (snc.Count() > 0)
            {
                foreach (NoteContent item in snc)
                {
                    Search sv = SearchTest(item, start, tzone);
                    if (sv.NoteID != start.NoteID)
                        return sv;
                }
            }

            // if none we go to next base

            List<BaseNoteHeader> sbnh = await NoteDataManager.GetSearchHeaders(_db, start, bnh);

            if (sbnh.Count() > 0)  // still have base notes to search
            {
                bnh = sbnh.First();
                start.ResponseOrdinal = 0;
                start.BaseOrdinal = bnh.NoteOrdinal;
                //start.NoteID = bnh.NoteID;
                myRespOrdinal = -1;
                goto Repeat;
            }

            return start;
        }


        //  Start of Sequencer code

        /// <summary>
        /// Begin running the sequencer
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> BeginSequence()     // begin first file in list with first unread note
        {
            string userid = _userManager.GetUserId(User);
            List<Sequencer> seqList = await NoteDataManager.GetSeqListForUser(_db, userid);

            if (seqList.Count == 0)
            {
                return RedirectToAction("Index", "Sequencers");
            }

            Sequencer myseqfile = await FirstSeq();

            if (myseqfile == null)
                return RedirectToAction("CompleteSequence");

            List<BaseNoteHeader> sbnh = await NoteDataManager.GetSbnh(_db, myseqfile);

            while (sbnh.Count() == 0)
            {
                myseqfile = await NextSeq();
                if (myseqfile == null)
                    return RedirectToAction("CompleteSequence");

                sbnh = await NoteDataManager.GetSbnh(_db, myseqfile);
            }

            BaseNoteHeader item = sbnh.First();
            return RedirectToAction("Display", new { id = item.NoteID });
        }



        /// <summary>
        /// Continue sequencing
        /// </summary>
        /// <param name="id">NoteID to start from</param>
        /// <returns></returns>
        public async Task<IActionResult> ContinueSequence(long id)      // next unread note in file
        {
            string userid = _userManager.GetUserId(User);
            List<Sequencer> list = await _db.Sequencer
                .Where(x => x.UserID == userid && x.Active)
                .OrderBy(x => x.Ordinal)
                .ToListAsync();

            Sequencer myseqfile = list.FirstOrDefault();

            var item0 = await NoteDataManager.GetNoteById(_db, id);

            BaseNoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, item0.NoteFileID, item0.NoteOrdinal);

            // check for reponses beyond current base/resp

            var myseqfile1 = myseqfile;
            var bnh1 = bnh;
            var snc = from x in _db.NoteContent
                      where x.NoteFileID == myseqfile1.NoteFileID && x.NoteOrdinal == bnh1.NoteOrdinal && x.ResponseOrdinal > item0.ResponseOrdinal && x.LastEdited >= myseqfile1.LastTime
                      orderby x.ResponseOrdinal
                      select new
                      {
                          noteid = x.NoteID   // get only NoteID
                      };

            if (bnh != null && snc != null && snc.Count() > 0)
            {
                long current = snc.First().noteid;
                return RedirectToAction("Display", new { id = current });
            }

            // else we go to next base note that qualifies

            List<BaseNoteHeader> sbnh = await NoteDataManager.GetSeqHeader1(_db, myseqfile, bnh);

            while (sbnh.Count() == 0)  // next file
            {
                myseqfile = await NextSeq();
                if (myseqfile == null)
                    return RedirectToAction("CompleteSequence");
                sbnh = await NoteDataManager.GetSeqHeader2(_db, myseqfile);

            }
            // next base note
            bnh = sbnh.First();
            return RedirectToAction("Display", new { id = bnh.NoteID });
        }

        // Inform user sequening is done
        public IActionResult CompleteSequence()
        {
            HttpContext.Session.Remove("CurrentSeq");
            return View();
        }
        
        /// <summary>
        /// Get first row from user's sequencer list
        /// </summary>
        /// <returns></returns>
        public async Task<Sequencer> FirstSeq()   // get first sequencer data line for user
        {
            HttpContext.Session.Remove("CurrentSeq");

            string userid = _userManager.GetUserId(User);
            List<Sequencer> list = await NoteDataManager.GetSeqListForUser(_db, userid);

            Sequencer item = list.First();
            item.StartTime = DateTime.Now.ToUniversalTime();
            item.Active = true;
            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("CurrentSeq", 1);
            return item;
        }

        /// <summary>
        /// Find next note that qualifies
        /// </summary>
        /// <returns></returns>
        public async Task<Sequencer> NextSeq()
        {
            bool found = false;
            HttpContext.Session.Remove("CurrentSeq");

            string userid = _userManager.GetUserId(User);
            List<Sequencer> mylist = await NoteDataManager.GetSeqListForUser(_db, userid);

            foreach (Sequencer item in mylist)
            {
                if (item.Active && !found) // end this file
                {
                    item.Active = false;
                    item.LastTime = item.StartTime;
                    _db.Entry(item).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    found = true;
                    HttpContext.Session.Remove("CurrentSeq");
                }
                else if (found) // start next file
                {
                    item.StartTime = DateTime.Now.ToUniversalTime();
                    item.Active = true;
                    _db.Entry(item).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    HttpContext.Session.SetInt32("CurrentSeq", 1);
                    return item;
                }
            }
            return null;
        }

        ///// <summary>
        ///// Export a file as txt, html (flat), html (expandable)
        ///// </summary>
        ///// <param name="id">NoteFileID</param>
        ///// <returns></returns>
        //public IActionResult Export(int id)
        //{
        //    NoteFile nf = db.NoteFile
        //        .Where(p => p.NoteFileID == id)
        //        .FirstOrDefault();

        //    // set up defaults
        //    NFViewModel model = new NFViewModel();
        //    model.isHtml = true;
        //    model.isCollapsible = true;
        //    model.directOutput = false;
        //    model.FileName = nf.NoteFileName;

        //    // send to view
        //    return View(model);
        //}

        /// <summary>
        /// Display NoteFile as expandable HTML
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <param name="id2">NoteOrdnal</param>
        /// <returns></returns>
        public async Task<IActionResult> AsHtml(int id, int id2)
        {
            NFViewModel model = new NFViewModel()
            {
                directOutput = true,
                isCollapsible = true,
                isHtml = true,
                NoteOrdinal = id2
            };
            NoteFile nfl = await NoteDataManager.GetFileById(_db, id);
            model.FileName = nfl.NoteFileName;

            ExportController exp = new ExportController(_appEnv, _userManager, _signInManager, _emailSender, _smsSender, _db, _telemetry);
            MemoryStream ms = await exp.DoExport(model, User);

            return new FileStreamResult(ms, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }

        public IActionResult MailFileAsHtml(int id, int id2)
        {
            ForwardView fv = new ForwardView()
            {
                IsAdmin = User.IsInRole("Admin"),
                toAllUsers = false,

                FileID = id,
                NoteOrdinal = id2
            };
            return View(fv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MailFileAsHtml(ForwardView fv)
        {
            bool toall = fv.toAllUsers;

            return RedirectToAction("MailAsHtml", new { id = fv.FileID, id2 = fv.NoteOrdinal, id3 = fv.ToEmail, id4 = toall });
        }

        public IActionResult MailStringAsHtml(int id, int id2)
        {
            ForwardView fv = new ForwardView()
            {
                FileID = id,
                NoteOrdinal = id2,
                IsAdmin = User.IsInRole("Admin"),
                toAllUsers = false
            };
            return View(fv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MailStringAsHtml(ForwardView fv)
        {
            bool toall = false;
            if (fv.toAllUsers)
                toall = true;

            return RedirectToAction("MailAsHtml", new { id = fv.FileID, id2 = fv.NoteOrdinal, id3 = fv.ToEmail, id4 =toall });
        }

        public async Task<IActionResult> MailAsHtml(int id, int id2, string id3, bool id4)
        {
            NFViewModel model = new NFViewModel()
            {
                directOutput = true,
                isCollapsible = true,
                isHtml = true,
                NoteOrdinal = id2
            };
            NoteFile nfl = await NoteDataManager.GetFileById(_db, id);
            model.FileName = nfl.NoteFileName;

            ExportController exp = new ExportController(_appEnv, _userManager, _signInManager, _emailSender, _smsSender, _db, _telemetry);
            MemoryStream ms = await exp.DoExport(model, User);

            StreamReader sr = new StreamReader(ms);
            string txt = await sr.ReadToEndAsync();

            await _emailSender.SendEmailAsync(id3, "From Notes 2017 - " + nfl.NoteFileName, txt);

            if(id4 && User.IsInRole("Admin"))
            {
                List<ApplicationUser> users = await _userManager.Users.ToListAsync();
                foreach (ApplicationUser user in users)
                {
                    await _emailSender.SendEmailAsync(user.Email, "From Notes 2017 - " + nfl.NoteFileName, txt);
                }
            }

            return RedirectToAction("Index", new {id }); 
        }

        /// <summary>
        /// Display NoteFile as "Flat" HTML
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <param name="id2">NoteOrdinal</param>
        /// <returns></returns>
        public async Task<IActionResult> AsHtmlAlt(int id, int id2)
        {
            ExportController exp = new ExportController(_appEnv, _userManager, _signInManager, _emailSender, _smsSender, _db, _telemetry);

            NFViewModel model = new NFViewModel()
            {
                directOutput = true,
                isCollapsible = false,
                isHtml = true,
                NoteOrdinal = id2
            };
            NoteFile nfl = await NoteDataManager.GetFileById(_db, id);
            model.FileName = nfl.NoteFileName;

            MemoryStream ms = await exp.DoExport(model, User);

            return new FileStreamResult(ms, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }

        public IActionResult Export(int id)
        {
            return RedirectToAction("PreExport", "Export", new {id });
        }

        public IActionResult Mark(long id)
        {
            List<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem() { Value = "-2", Text = "-- Select an Option --" },
                new SelectListItem() { Value = "-1", Text = "Entire Note String" },
                new SelectListItem() { Value = "0", Text = "Base Note" },
                new SelectListItem() { Value = "1", Text = "Response" }
            };
            MarkView v = new MarkView()
            {
                SelectedValue = items.First().Value,
                option = items,
                NoteID = id
            };
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkProc(MarkView markview)
        {
            // Get NoteContent
            NoteContent nc = await NoteDataManager.GetNoteById(_db, markview.NoteID);

            BaseNoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, nc.NoteFileID, nc.NoteOrdinal);

            Mark markx = new Mark()
            {
                BaseNoteHeaderID = bnh.BaseNoteHeaderID,
                NoteFileID = nc.NoteFileID,
                NoteOrdinal = nc.NoteOrdinal,
                UserID = _userManager.GetUserId(User)
            };
            var markx1 = markx;
            var q = _db.Mark
                .Where(p => p.UserID == markx1.UserID);

            int ord = q.Count() + 1;

            markx.MarkOrdinal = ord;

            if (markview.SelectedValue == "-1")
            {
                markx.ResponseOrdinal = 0;
                _db.Mark.Add(markx);

                for ( int i = 1; i <= bnh.Responses ; i++ )
                {
                    markx = new Mark()
                    {
                        BaseNoteHeaderID = bnh.BaseNoteHeaderID,
                        NoteFileID = nc.NoteFileID,
                        NoteOrdinal = nc.NoteOrdinal,
                        UserID = _userManager.GetUserId(User),
                        MarkOrdinal = ++ord,
                        ResponseOrdinal = i
                    };
                    _db.Mark.Add(markx);
                }
            }
            else if (markview.SelectedValue == "0")
            {
                markx.ResponseOrdinal = 0;
                _db.Mark.Add(markx);
            }
            else if (markview.SelectedValue == "1")
            {
                markx.ResponseOrdinal = nc.ResponseOrdinal;
                _db.Mark.Add(markx);
            }

            await _db.SaveChangesAsync();

            return View("Marked");
        }

    }
}
