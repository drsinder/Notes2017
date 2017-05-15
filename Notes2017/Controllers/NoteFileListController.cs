/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteFileListController.cs
**
**  Description:
**      NoteFile List Controller for Notes 2017
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2017.Models;
using Notes2017.Services;
using Microsoft.AspNetCore.Http;
using Notes2017.Data;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "User")]
    public class NoteFileListController : NController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public NoteFileListController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ApplicationDbContext applicationDbContext) : base(userManager, signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = applicationDbContext;
        }


        // GET: NoteFileList
        /// <summary>
        /// Display list of NoteFiles
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetInt32("IsSearch", 0);

            TZone tz = await TZone.GetUserTimeZone(Request.HttpContext, User, _userManager, _signInManager, _db);
            ViewBag.TZ = tz;

            List<NoteFile> nf = await NoteDataManager.GetNoteFilesOrderedByName(_db);
            return View(nf);
        }

        // GET: NoteFileList/Details/5
        /// <summary>
        /// Show some info about the NoteFile
        /// </summary>
        /// <param name="id">NoteFileID</param>
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

            TZone tz = await TZone.GetUserTimeZone(Request.HttpContext, User, _userManager, _signInManager, _db);
            ViewBag.TZ = tz;

            ViewBag.BaseNotes = await NoteDataManager.NextBaseNoteOrdinal(_db, (int)id) - 1;
            ViewBag.TotalNotes = await NoteDataManager.GetNumberOfNotes(_db, (int)id);
            return View(noteFile);
        }

        /// <summary>
        /// Shim to Enter a NoteFile from the list of NoteFiles
        /// </summary>
        /// <param name="id">NoteFile</param>
        /// <returns></returns>
        public async Task<IActionResult> Viewit(int? id)
        {
            HttpContext.Session.SetInt32("IsSearch", 0);

            if (id == null)
            {
                return NotFound();
            }

            NoteFile noteFile = await NoteDataManager.GetFileById(_db, (int)id);

            if (noteFile == null)
            {
                return NotFound();
            }

            // Check access
            NoteAccess nacc = await GetMyAccess((int)id);
            if (!nacc.Write && !nacc.Read) // can not read or write = no access
                return RedirectToAction("Index");

            return RedirectToAction("Index", "NoteDisplay", new { id = noteFile.NoteFileID });
        }

        /// <summary>
        /// Get Access for User in a file
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public async Task<NoteAccess> GetMyAccess(int fileid)
        {
            NoteAccess noteAccess = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), fileid);
            ViewData["MyAccess"] = noteAccess;
            return noteAccess;
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        // db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
