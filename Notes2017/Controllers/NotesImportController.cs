/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NotesImportController.cs
**
**  Description:
**      Notes Import Controller for Notes 2017
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
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2017.Models;
using Microsoft.ApplicationInsights;
using Notes2017.App_Code;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NotesImportController : NController
    {
        private readonly IHostingEnvironment _appEnv;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        private readonly TelemetryClient _telemetry;

        private const char Ff = (char)(12); //  FF

        //private const char TAB = (char)(9); //  Tab


        public NotesImportController(
            IHostingEnvironment appEnv,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext applicationDbContext, TelemetryClient tel) : base(userManager, signInManager)
        {
            _appEnv = appEnv;
            _userManager = userManager;
            _db = applicationDbContext;

            _telemetry = tel;
            _telemetry.InstrumentationKey = Global.InstKey;
        }

        // GET: NotesImport
        /// <summary>
        /// Display list of NoteFiles user can import to
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return View(await NoteDataManager.GetNoteFilesOrderedByName(_db));
        }

        public async Task<IActionResult> StartImport(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            NoteFile noteFile = await NoteDataManager.GetFileById(_db, (int)id);

            if (noteFile == null)
            {
                return NotFound();
            }

            return View(noteFile);
        }

        // GET: NotesImport/Edit/5
        /// <summary>
        /// Process import of NoteFile from a like named .txt file
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(NoteFile nf)
        {
            if (nf == null)
            {
                return NotFound();
            }
            NoteFile noteFile =  await NoteDataManager.GetFileById(_db, nf.NoteFileID);

            //TZone tzone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            if (noteFile == null)
            {
                return NotFound();
            }

            _telemetry.TrackEvent("Start Import of file = " + noteFile.NoteFileName);

            int id = noteFile.NoteFileID;
            long counter = 0;
            bool isResp = false;
            char[] spaceTrim = new char[] { ' ' };
            char[] slash = new char[] { '/' };
            char[] colon = new char[] { ':' };
            char[] dash = new char[] { '-' };

            string platoBaseYear = "";

            StringBuilder sb = new StringBuilder();
            long baseNoteHeaderId = 0;
            NoteContent nc = null;
            int filetype = 0;  // 0= NovaNET | 1 = Notes 3.1 | 2 = plato iv group notes -- we can process three formats

            string fileName = _appEnv.ContentRootPath + "\\wwwroot\\ImportFiles\\" + noteFile.NoteFileName + ".txt";

            // Read the file and process it line by line.
            StreamReader file = new StreamReader(fileName);
            try
            {
                BaseNoteHeader bnh;
                NoteContent newNote;
                string line;
                while ((line = await file.ReadLineAsync()) != null)
                {
                    if (counter == 0)
                    {
                        if (line.StartsWith("NoteFile "))  // By this we know it came from Notes 3.1
                        {
                            filetype = 1;   // Notes 3.1
                            await file.ReadLineAsync();
                            line = await file.ReadLineAsync();
                        }
                        else if (line.StartsWith("+++ plato iv group notes +++"))
                        {
                            filetype = 2;   // plato iv group notes
                            await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            var platoLine5 = await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            await file.ReadLineAsync();

                            await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            await file.ReadLineAsync();

                            string[] splits = platoLine5.Split(spaceTrim);
                            platoBaseYear = splits[splits.Length - 1];

                        }
                    }

                    if (filetype == 0)  // Process for NovaNET output
                    {

                        line = await CheckFf(line, file);
                        if (line.Length > 52)  // Possible Note Header
                        {
                            string head = line.Substring(46);
                            if (head.StartsWith("  Note ")) //new note found
                            {
                                if (nc != null)  // have a note to write
                                {
                                    nc.NoteBody = sb.ToString();
                                    sb = new StringBuilder();
                                    nc.NoteBody = nc.NoteBody.Replace("\r\n", "<br />");

                                    if (!isResp) // base note
                                    {
                                        //basenotes++;
                                        newNote = await NoteDataManager.CreateNote(_db, _userManager, nc, false);
                                        BaseNoteHeader baseNoteHeader = await GetBaseNoteHeader(newNote);
                                        baseNoteHeaderId = baseNoteHeader.BaseNoteHeaderID;

                                        baseNoteHeader.CreateDate = newNote.LastEdited;
                                        _db.Entry(baseNoteHeader).State = EntityState.Modified;
                                        await _db.SaveChangesAsync();
                                    }
                                    else // resp
                                    {
                                        bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                                        await NoteDataManager.CreateResponse(_db, _userManager, bnh, nc, false);
                                    }
                                }
                                nc = new NoteContent {NoteFileID = id};

                                // get title at start of line
                                var title = line.Substring(0, 40).TrimEnd(spaceTrim);
                                nc.NoteSubject = title;
                                isResp = head.Contains("Response");  // is this a response?

                                line = file.ReadLine();     // get next line  
                                line = await CheckFf(line, file);
                                if (isResp && line.StartsWith("----")) // perhaps a response title
                                {
                                    title = line.Substring(4, line.Length - 4).TrimEnd(spaceTrim);
                                    nc.NoteSubject = title;
                                    line = await file.ReadLineAsync();  // skip line
                                    await CheckFf(line, file);
                                }
                                else if (isResp)
                                    nc.NoteSubject = "*none*";  // must have a title

                                if (string.IsNullOrEmpty(nc.NoteSubject))  // must have a title
                                    nc.NoteSubject = "*none*";

                                line = await file.ReadLineAsync();  // skip line
                                line = await CheckFf(line, file);

                                // Check for possible director message 
                                if (line.StartsWith("    "))
                                {
                                    nc.DirectorMesssage = line.Trim(spaceTrim);
                                    line = await file.ReadLineAsync();  // skip line
                                    await CheckFf(line, file);
                                    line = await file.ReadLineAsync();  // skip line
                                    line = await CheckFf(line, file);
                                }


                                // get date, time, author
                                // first date
                                string date = line.Substring(0, 10).TrimEnd(spaceTrim);
                                string[] x = date.Split(slash);

                                string prefix = "/20";
                                int yearpart = int.Parse(x[2]);
                                if (yearpart > 70)
                                    prefix = "/19";

                                string datetime = (x[0].Length == 1 ? "0" + x[0] : x[0]) + "/" + (x[1].Length == 1 ? "0" + x[1] : x[1]) + prefix + x[2];

                                // now time
                                string time = line.Substring(10, 6);
                                time = time.Trim(spaceTrim);
                                time = time.Replace("am", " ");
                                time = time.Replace("pm", " ");
                                time = time.Replace("a", " ");
                                time = time.Replace("p", " ");
                                time = time.TrimEnd(spaceTrim);

                                string[] y = time.Split(colon);
                                int hour = int.Parse(y[0]);
                                if (line.Substring(0, 23).Contains("pm") && hour < 12)
                                    hour = hour + 12;

                                datetime += " " + ((hour < 10) ? "0" + hour.ToString() : hour.ToString()) + ":" + y[1];

                                // Save 
                                nc.LastEdited = DateTime.Parse(datetime);

                                //nc.LastEdited = tzone.Universal(nc.LastEdited);

                                // author
                                nc.AuthorName = line.Substring(25).Trim(spaceTrim);
                                nc.AuthorID = Global.ImportedAuthorId();   //"imported";
                                line = await file.ReadLineAsync();  // skip line
                                line = await CheckFf(line, file);
                            }
                        }
                        // append line to current note
                        sb.AppendLine(line);
                        counter++;
                    }  // end NovaNET

                    else if (filetype == 1)  // Process from Notes 3.1 export as text
                    {
                        if (line.StartsWith("Note: "))  // possible note header
                        {
                            string[] parts = line.Split(dash);
                            if (parts.Length >= 5)  // looks like the right number of sections > with - in subject grrr
                            {
                                if (nc != null)  // have a note to write
                                {
                                    sb.Append(" ");
                                    nc.NoteBody = sb.ToString();
                                    sb = new StringBuilder();
                                    //nc.NoteBody = nc.NoteBody.Replace("\r\n", "<br />");

                                    if (!isResp) // base note
                                    {
                                        //basenotes++;
                                        newNote = await NoteDataManager.CreateNote(_db, _userManager, nc, false);
                                        BaseNoteHeader baseNoteHeader = await GetBaseNoteHeader(newNote);
                                        baseNoteHeaderId = baseNoteHeader.BaseNoteHeaderID;

                                        baseNoteHeader.CreateDate = newNote.LastEdited;
                                        _db.Entry(baseNoteHeader).State = EntityState.Modified;
                                        await _db.SaveChangesAsync();
                                    }
                                    else // resp
                                    {
                                        bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                                        await NoteDataManager.CreateResponse(_db, _userManager, bnh, nc, false);
                                    }
                                }
                                nc = new NoteContent {NoteFileID = id};

                                // parse sections, 0 is note number, 1 is subject, 2 is author, 3 is datetime, 4 is number of responses
                                // skip section 0
                                // Get subject

                                string part = parts[1];
                                if (part.StartsWith(" Subject: "))
                                {
                                    if (parts.Length == 5)
                                        nc.NoteSubject = part.Substring(9, part.Length - 9).Trim() + " ";  //only works if no - in subject grrr
                                    else
                                    {
                                        int subjectindx = line.IndexOf(" - Subject: ", StringComparison.Ordinal);
                                        int authorindx = line.IndexOf(" - Author: ", StringComparison.Ordinal);
                                        nc.NoteSubject = subjectindx < authorindx ? 
                                            line.Substring(subjectindx + 12, authorindx - subjectindx - 12).Trim() 
                                            : "** Subject Parse Error **";
                                    }
                                }
                                // get author
                                part = parts[parts.Length - 3];
                                if (part.StartsWith(" Author: "))
                                {
                                    nc.AuthorName = part.Substring(8, part.Length - 8).Trim();
                                    nc.AuthorID = "Import";
                                }
                                else
                                {
                                    nc.AuthorName = "** Author Parse Error **";
                                    nc.AuthorID = "Import";
                                }
                                part = parts[parts.Length - 2].Trim();
                                nc.LastEdited = DateTime.Parse(part);
                                // skip resp

                                line = await file.ReadLineAsync();
                                if (line.StartsWith("Tags -"))
                                {
                                    isResp = false;
                                    nc.DirectorMesssage = line.Substring(6, line.Length - 6).Trim();
                                }
                                else if (line.StartsWith("Base Note Subject: "))
                                {
                                    isResp = true;  // but skip content
                                    line = await file.ReadLineAsync();  // Should be Tag
                                    if (line.StartsWith("Tags -"))
                                    {
                                        nc.DirectorMesssage = line.Substring(6, line.Length - 6).Trim();
                                    }
                                }
                                await file.ReadLineAsync();  //skip a line
                                line = await file.ReadLineAsync();  //first content line
                            }
                        }
                        // append line to current note
                        sb.AppendLine(line);
                        counter++;
                    }  // end Notes 3.1

                    else if (filetype ==2)  // PLATO
                    {

                        int xflag = 0;
                        if (line.Contains("-------- note "))  //  note header
                            xflag = 1;
                        else if (line.Contains("-------- response "))  //  response header
                            xflag = 2;

                        if (xflag > 0) // we have note to write (maybe)
                        {
                            if (nc != null)  // have a note to write
                            {
                                nc.NoteBody = sb.ToString();
                                sb = new StringBuilder();
                                nc.NoteBody = nc.NoteBody.Replace("\r\n", "<br />");

                                if (!isResp) // base note
                                {
                                    //basenotes++;
                                    newNote = await NoteDataManager.CreateNote(_db, _userManager, nc, false);
                                    BaseNoteHeader baseNoteHeader = await GetBaseNoteHeader(newNote);
                                    baseNoteHeaderId = baseNoteHeader.BaseNoteHeaderID;

                                    baseNoteHeader.CreateDate = newNote.LastEdited;
                                    _db.Entry(baseNoteHeader).State = EntityState.Modified;
                                    await _db.SaveChangesAsync();

                                }
                                else // resp
                                {
                                    bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                                    await NoteDataManager.CreateResponse(_db, _userManager, bnh, nc, false);
                                }
                            }
                            nc = new NoteContent {NoteFileID = id};

                            // now start with new note proc

                            if (xflag == 1) // get note title
                            {
                                // mark we have a base note / get title

                                line = line.Substring(16);
                                string[] splitsx = line.Split(spaceTrim);
                                nc.NoteSubject = line.Substring(splitsx[0].Length).Trim(spaceTrim);

                                isResp = false;
                            }
                            else if (xflag == 2)
                            {
                                isResp = true;  // mark response
                                nc.NoteSubject = "*none*";  // must have a title
                            }
                            if (string.IsNullOrEmpty(nc.NoteSubject))  // must have a title
                                nc.NoteSubject = "*none*";

                            line = await file.ReadLineAsync(); // move to info header

                            // process header

                            // count the /s to get date format.

                            int cnt = Regex.Matches(line, "/").Count;
                            if (cnt < 1) // try next line grrr.
                            {
                                line = await file.ReadLineAsync(); // move to info header
                                cnt = Regex.Matches(line, "/").Count;
                            }

                            line = " " + line;
                            line = line.Replace("     ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");
                            line = line.Replace("     ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");


                            string[] splits = line.Split(' ', '/', '.', ':', ',', ';');

                            string date;
                            if (cnt == 1)
                            {
                                date = splits[1] + "/" + splits[2] + "/" + platoBaseYear + " "
                                    + splits[3] + ":" + splits[4] + ":00";
                            }
                            else
                            {
                                date = splits[1] + "/" + splits[2] + "/19" + splits[3] + " "
                                    + splits[4] + ":" + splits[5] + ":00";
                            }
                            nc.LastEdited = DateTime.Parse(date);

                            string group = " " + splits[splits.Length - 1];
                            string name = "";
                            for ( int i = 4+cnt; i < splits.Length - 1; i++)
                            {
                                name += splits[i] + " ";
                            }

                            nc.AuthorName = name + "/" + group;
                            nc.AuthorID = Global.ImportedAuthorId();   //"imported";

                            await file.ReadLineAsync(); // skip lines to get to content
                            line = await file.ReadLineAsync(); // skip lines to get to content
                        }

                        //// pre proc above
                        sb.AppendLine(line); // collect lines of note
                        counter++;
                    }  // end PLATO
                }  // end where


                if (filetype == 0)  // NovaNET
                {
                    if (nc != null)  // have a note to write
                    {
                        nc.NoteBody = sb.ToString();
                        sb.Clear();

                        nc.NoteBody = nc.NoteBody.Replace("\r\n", "<br />");


                        if (!isResp) // base note
                        {
                            //basenotes++;
                            newNote = await NoteDataManager.CreateNote(_db, _userManager, nc, false);
                            BaseNoteHeader baseNoteHeader = await GetBaseNoteHeader(newNote);
                            //baseNoteHeaderId = baseNoteHeader.BaseNoteHeaderID;

                            baseNoteHeader.CreateDate = newNote.LastEdited;
                            _db.Entry(baseNoteHeader).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                        }
                        else // resp
                        {
                            bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                            await NoteDataManager.CreateResponse(_db, _userManager, bnh, nc, false);
                        }
                    }
                }
                else if (filetype == 1)  // Notes 3.1
                {
                    if (nc != null)  // have a note to write
                    {
                        sb.Append(" ");
                        nc.NoteBody = sb.ToString();
                        sb.Clear();

                        if (!isResp) // base note
                        {
                            //basenotes++;
                            newNote = await NoteDataManager.CreateNote(_db, _userManager, nc, false);
                            BaseNoteHeader baseNoteHeader = await GetBaseNoteHeader(newNote);
                            //baseNoteHeaderId = baseNoteHeader.BaseNoteHeaderID;

                            baseNoteHeader.CreateDate = newNote.LastEdited;
                            _db.Entry(baseNoteHeader).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                        }
                        else // resp
                        {
                            bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                            await NoteDataManager.CreateResponse(_db, _userManager, bnh, nc, false);
                        }
                    }
                }
                else if (filetype == 2)  // PLATO
                {
                    if (nc != null)  // have a note to write
                    {
                        nc.NoteBody = sb.ToString();
                        sb.Clear();
                        nc.NoteBody = nc.NoteBody.Replace("\r\n", "<br />");

                        if (!isResp) // base note
                        {
                            //basenotes++;
                            newNote = await NoteDataManager.CreateNote(_db, _userManager, nc, false);
                            BaseNoteHeader baseNoteHeader = await GetBaseNoteHeader(newNote);
                            //baseNoteHeaderId = baseNoteHeader.BaseNoteHeaderID;

                            baseNoteHeader.CreateDate = newNote.LastEdited;
                            _db.Entry(baseNoteHeader).State = EntityState.Modified;
                            await _db.SaveChangesAsync();

                        }
                        else // resp
                        {
                            bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                            await NoteDataManager.CreateResponse(_db, _userManager, bnh, nc, false);
                        }
                    }

                }

            }
            catch (Exception)
            {
                // ignored
            }

            file.Close();

            _telemetry.TrackEvent("Finish Import of file = " + noteFile.NoteFileName);

            return RedirectToAction("Index", "NoteDisplay", new  {id });
        }

        /// <summary>
        /// Process NovaNET formfeed
        /// </summary>
        /// <param name="inline">input line</param>
        /// <param name="file">StreamReader for import file</param>
        /// <returns></returns>
        public async Task<string> CheckFf(string inline, StreamReader file)
        {
            if (inline.Length == 1)
            {
                char[] mychars = inline.ToCharArray();
                if (mychars[0] == Ff)
                {
                    await file.ReadLineAsync();
                    await file.ReadLineAsync();
                    await file.ReadLineAsync();
                    await file.ReadLineAsync();
                    return await file.ReadLineAsync();
                }
            }
            return inline;
        }


        //public async Task<string> CheckFFPLATO(string inline, StreamReader file)
        //{
        //    string line;
        //    if (inline.Length == 1)
        //    {
        //        char[] mychars = inline.ToCharArray();
        //        if (mychars[0] == FF)
        //        {
        //            line = await file.ReadLineAsync();
        //            line = await file.ReadLineAsync();
        //            line = await file.ReadLineAsync();
        //            return line;
        //        }
        //    }
        //    return inline;
        //}

        /// <summary>
        /// Get the BaseNoteHeader for a NoteContent
        /// </summary>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        public async Task<BaseNoteHeader> GetBaseNoteHeader(NoteContent nc)
        {
            return await NoteDataManager.GetBaseNoteHeader(_db, nc.NoteFileID, nc.NoteOrdinal);
        }

        //public async Task<IActionResult> ImportTimeZones()
        //{
        //    string fileName = _appEnv.ApplicationBasePath + "\\ImportFiles\\"  + "TimeZones.txt";

        //    string line;
        //    // Read the file and process it line by line.
        //    StreamReader file = new StreamReader(fileName);
        //    List <TZone> tzl = new List<TZone>();
        //    try
        //    {
        //        while ((line = await file.ReadLineAsync()) != null)
        //        {
        //            string[] splits = line.Split(new char[] { TAB });
        //            int cnt = splits.Length;
        //            TZone tz = new TZone();
        //            tz.Abbreviation = splits[0].Trim();
        //            tz.Name = splits[1].Trim();
        //            tz.Offset = splits[2].Trim();
        //            string off = tz.Offset.Replace("UTC", "");
        //            string[] s2 = off.Split(new char[] { ':' });
        //            tz.OffsetHours = int.Parse(s2[0]);
        //            tz.OffsetMinutes = 0;
        //            if (s2.Length == 2)
        //            {
        //                tz.OffsetMinutes = int.Parse(s2[1]);
        //                if (tz.OffsetHours < 0)
        //                    tz.OffsetMinutes = -tz.OffsetMinutes;
        //            }
        //            tzl.Add(tz);
        //        }
        //    }
        //    catch { }

        //    file.Close();

        //    db.TZone.AddRange(tzl);
        //    await db.SaveChangesAsync();

        //    return RedirectToAction("Index", "Home");
        //}

    }
}
