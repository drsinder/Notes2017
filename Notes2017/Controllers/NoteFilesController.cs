/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteFilesController.cs
**
**  Description:
**      NoteFiles Controller for Notes 2017
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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2017.Models;
using Microsoft.ApplicationInsights;
using Notes2017.App_Code;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NoteFilesController : NController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly TelemetryClient _telemetry;

        public NoteFilesController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext applicationDbContext, TelemetryClient tel) : base(userManager, signInManager)
        {
            _userManager = userManager;
            _db = applicationDbContext;

            _telemetry = tel;
            _telemetry.InstrumentationKey = Global.InstKey;
        }

        /// <summary>
        /// Create a New NoteFile with default Access Controls.
        /// "Other" has no access.  Creator has full access.
        /// </summary>
        /// <param name="name">Name for the file</param>
        /// <param name="title">Title of the file</param>
        /// <returns></returns>
        public async Task<bool> CreateNoteFile(string name, string title)
        {
            await AccessManager.Audit(_db, "Create", User.Identity.Name, _userManager.GetUserId(User), "Create NotesFile " + name, _telemetry);

            return await NoteDataManager.CreateNoteFile(_db, _userManager, _userManager.GetUserId(User), name, title);
        }

        //public void CheckBaseNotefiles()
        //{
        //    CreateNoteFile("announce", "Notes 2017 Announcements");
        //    CreateNoteFile("pbnotes", "Public Notes");
        //    CreateNoteFile("noteshelp", "Help with Notes 2017");

        //    CreateNoteFile("pad", "Traditional CERL Pad");
        //    CreateNoteFile("padofold", "Last Notes from Pad on NovaNet");

        //}

        // GET: NoteFiles
        /// <summary>
        /// Display list of NoteFiles
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            NoteFile nf1 = await NoteDataManager.GetFileByName(_db, "announce");
            NoteFile nf2 = await NoteDataManager.GetFileByName(_db, "pbnotes");
            NoteFile nf3 = await NoteDataManager.GetFileByName(_db, "noteshelp");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "pad");

            ViewBag.announce = nf1 == null;
            ViewBag.pbnotes = nf2 == null;
            ViewBag.noteshelp = nf3 == null;
            ViewBag.pad = nf4 == null;

            return View(await NoteDataManager.GetNoteFilesOrderedByName(_db));
        }

        public async Task<IActionResult> CreateAnnounce()
        {
            await CreateNoteFile("announce", "Notes 2017 Announcements");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "announce");
            int padid = nf4.NoteFileID;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Global.AccessOther(), padid);
            access.Read = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreatePbnotes()
        {
            await CreateNoteFile("pbnotes", "Public Notes");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "pbnotes");
            int padid = nf4.NoteFileID;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Global.AccessOther(), padid);
            access.Read = true;
            access.Respond = true;
            access.Write = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreateNoteshelp()
        {
            await CreateNoteFile("noteshelp", "Help with Notes 2017");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "noteshelp");
            int padid = nf4.NoteFileID;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Global.AccessOther(), padid);
            access.Read = true;
            access.Respond = true;
            access.Write = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreatePad()
        {
            await CreateNoteFile("pad", "Traditional Pad");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "pad");
            int padid = nf4.NoteFileID;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Global.AccessOther(), padid);
            access.Read = true;
            access.Respond = true;
            access.Write = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: NoteFiles/Details/5
        /// <summary>
        /// Dislplay details about a file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
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
            ViewBag.BaseNotes = await NoteDataManager.NextBaseNoteOrdinal(_db, (int)id) - 1;
            ViewBag.TotalNotes = await NoteDataManager.GetNumberOfNotes(_db, (int)id);
            ViewBag.TotalLength = await NoteDataManager.GetFileLength(_db, (int)id);

            return View(noteFile);
        }

        // GET: NoteFiles/Create
        /// <summary>
        /// Send to view to collect info for new file
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View();
        }

        // POST: NoteFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Process creation of a new NoteFile
        /// </summary>
        /// <param name="noteFile">NoteFile data from View</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(/*[Bind(Include = "NoteFileID,NoteFileName,NoteFileTitle,LastEdited")]*/ NoteFile noteFile)
        {
            if (ModelState.IsValid)
            {
                await AccessManager.Audit(_db, "Create", User.Identity.Name, _userManager.GetUserId(User), "Create NotesFile " + noteFile.NoteFileName, _telemetry);

                if (!await NoteDataManager.CreateNoteFile(_db, _userManager, _userManager.GetUserId(User), noteFile.NoteFileName, noteFile.NoteFileTitle))
                    return View(noteFile);

                await AccessManager.Audit(_db, "Failed", User.Identity.Name, _userManager.GetUserId(User), "Create NotesFile " + noteFile.NoteFileName, _telemetry);

                return RedirectToAction("Index");
            }

            return View(noteFile);
        }

        // GET: NoteFiles/Edit/5
        /// <summary>
        /// Set up to edit a NoteFile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
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

        // POST: NoteFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Process Edit of NoteFile
        /// </summary>
        /// <param name="noteFile"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NoteFile noteFile)
        {
            if (ModelState.IsValid)
            {
                await AccessManager.Audit(_db, "Edit", User.Identity.Name, _userManager.GetUserId(User), "Edit NotesFile " + noteFile.NoteFileName, _telemetry);
                _db.Entry(noteFile).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(noteFile);
        }

        // GET: NoteFiles/Delete/5
        /// <summary>
        /// Setup to  delete a NoteFile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
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

        // POST: NoteFiles/Delete/5
        /// <summary>
        /// Process deletion of notefile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            NoteFile noteFile = await NoteDataManager.GetFileById(_db, id);
            await AccessManager.Audit(_db, "Delete", User.Identity.Name, _userManager.GetUserId(User), "Delete NotesFile " + noteFile.NoteFileName, _telemetry);

            await NoteDataManager.DeleteNoteFile(_db, id);
            return RedirectToAction("Index");
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        //db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
