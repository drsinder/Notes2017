/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name:DataSpyController.cs
**
**  Description:
**      DataSpy Controller for Notes 2017
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

using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Notes2017.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Notes2017.Services;
using Microsoft.ApplicationInsights;
using Notes2017.App_Code;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DataSpyController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TelemetryClient _telemetry;

        private readonly ApplicationDbContext _context;

        public DataSpyController(
            IHostingEnvironment appEnv,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender, 
            ApplicationDbContext context,
            SQLFileDbContext sqlcontext, 
            TelemetryClient tel)
        {
            _userManager = userManager;
            _context = context;

            _telemetry = tel;
            _telemetry.InstrumentationKey = Global.InstKey;
        }

        // GET: DataSpy
        public async Task<IActionResult> Index()
        {
            await AccessManager.Audit(_context, "Index", User.Identity.Name,
                User.Identity.Name, "View list of NotesFiles", _telemetry);
            return View(await _context.NoteFile.ToListAsync());
        }

        // GET: DataSpy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NoteFile noteFile = await _context.NoteFile.SingleAsync(m => m.NoteFileID == id);
            if (noteFile == null)
            {
                return NotFound();
            }

            await AccessManager.Audit(_context, "Details", User.Identity.Name,
                User.Identity.Name, "View details of NotesFile " + noteFile.NoteFileName, _telemetry);

            return View(noteFile);
        }

        public async Task<IActionResult> Headers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            List<BaseNoteHeader> headers = await NoteDataManager.GetBaseNoteHeadersForFile(_context, (int)id);

            await AccessManager.Audit(_context, "Headers", User.Identity.Name,
                User.Identity.Name, "View BaseNoteHeaders of NotesFileID " + id, _telemetry);

            return View(headers);
        }

        public async Task<IActionResult> Content(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            List<NoteContent> content = await NoteDataManager.GetAllNoteContentList(_context, (int)id);

            await AccessManager.Audit(_context, "Content", User.Identity.Name,
               User.Identity.Name, "View NoteContent of NotesFileID " + id, _telemetry);

            return View(content);
        }

        public async Task<IActionResult> Access(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            List<NoteAccess> access = await _context.NoteAccess.Where(m => m.NoteFileID == id).ToListAsync();

            await AccessManager.Audit(_context, "Access", User.Identity.Name,
                User.Identity.Name, "View NoteAccess of NotesFileID " + id, _telemetry);

            return View(access);
        }

        public async Task<IActionResult> Marks()
        {
            List<Mark> mark = await _context.Mark.OrderBy(p => p.UserID).ToListAsync();

            await AccessManager.Audit(_context, "Marks", User.Identity.Name,
                User.Identity.Name, "View Marks", _telemetry);

            return View(mark);
        }

        public async Task<IActionResult> Search()
        {
            List<Search> s = await _context.Search.OrderBy(p => p.UserID).ToListAsync();

            await AccessManager.Audit(_context, "SearchView", User.Identity.Name,
                User.Identity.Name, "View SearchView", _telemetry);

            return View(s);
        }

        public async Task<IActionResult> Sequencer()
        {
            List<Sequencer> s = await _context.Sequencer.OrderBy(p => p.UserID).ToListAsync();

            await AccessManager.Audit(_context, "Sequencer", User.Identity.Name,
                User.Identity.Name, "View Sequencer", _telemetry);
            return View(s);
        }

        public async Task<IActionResult> Words()
        {
            List<Words> s = await _context.Words.OrderBy(p => p.ListNum).ThenBy(p => p.UserName).ThenBy(p => p.Word).ToListAsync();

            await AccessManager.Audit(_context, "Words", User.Identity.Name,
                User.Identity.Name, "View Words", _telemetry);

            return View(s);
        }

        public async Task<IActionResult> Users()
        {
            List<ApplicationUser> s = await _userManager.Users.ToListAsync();    //_context.ApplicationUser.ToListAsync();

            await AccessManager.Audit(_context, "Users", User.Identity.Name,
                User.Identity.Name, "View Users", _telemetry);

            return View("Users2", s);
        }

        public async Task<IActionResult> Audit()
        {
            List<Audit> s = await _context.Audit.OrderByDescending(p => p.AuditID).ToListAsync();

            await AccessManager.Audit(_context, "Audit", User.Identity.Name,
                User.Identity.Name, "View Audit", _telemetry);

            return View(s);
        }

        public async Task<IActionResult> EditContent(long id)
        {
            NoteContent model = await NoteDataManager.GetNoteById(_context, id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContent(NoteContent nc)
        {
            NoteContent edited = await NoteDataManager.GetNoteById(_context, nc.NoteID);

            edited.NoteSubject = nc.NoteSubject;
            edited.NoteBody = nc.NoteBody;

            await AccessManager.Audit(_context, "EditContent", User.Identity.Name,
                User.Identity.Name, "Edit Content NoteID " + edited.NoteID + " FileID " + edited.NoteFileID, _telemetry);

            _context.Entry(edited).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RedirectToAction("Content", new { id = nc.NoteFileID });
        }

        public async Task<IActionResult> TimeZones()
        {
            List<TZone> s = await _context.TZone.OrderBy(p => p.OffsetMinutes).ThenBy(p => p.OffsetHours).ToListAsync();

            await AccessManager.Audit(_context, "TZone", User.Identity.Name,
                User.Identity.Name, "View TZone", _telemetry);

            return View(s);
        }

        // GET: TZonesZZ/Edit/5
        public async Task<IActionResult> EditTZ(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TZone tZone = await _context.TZone.SingleAsync(m => m.TZoneID == id);
            if (tZone == null)
            {
                return NotFound();
            }
            return View(tZone);
        }

        // POST: TZonesZZ/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTZ(TZone tZone)
        {
            if (ModelState.IsValid)
            {
                await AccessManager.Audit(_context, "EditTimeZone", User.Identity.Name,
                    User.Identity.Name, "Edit TimeZone " + tZone.TZoneID, _telemetry);

                _context.Update(tZone);
                await _context.SaveChangesAsync();
                return RedirectToAction("TimeZones");
            }
            return View(tZone);
        }


    }
}
