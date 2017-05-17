/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: HomeController.cs
**
**  Description:
**      Home Controller for Notes 2017
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Notes2017.Data;
using Notes2017.Models;
using Notes2017.Services;
using Notes2017.ViewModels;
using PusherServer;

namespace Notes2017.Controllers
{
    public class HomeController : NController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        private readonly string _stylePath;

        public HomeController(
           IHostingEnvironment appEnv,
           UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ApplicationDbContext applicationDbContext) : base(userManager, signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = applicationDbContext;

            _stylePath = appEnv.ContentRootPath + "\\wwwroot\\css\\ExportCSS\\Customizable.css";
        }

        /// <summary>
        /// Root of the App / Home Page
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                TZone tzone = await TZone.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);
                ViewBag.UpdateClock = false;

                if (_signInManager.IsSignedIn(User))
                {
                    ApplicationUser user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
                    HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(user.Pref1));

                    Jobs job = new Jobs();
                    if (user.Pref2)
                    {
                        ViewBag.UpdateClock = true;
                        RecurringJob.AddOrUpdate(User.Identity.Name, () => job.UpdateHomePageTime(User.Identity.Name, tzone), Cron.Minutely);
                    }
                    else
                    {
                        RecurringJob.RemoveIfExists(User.Identity.Name);
                    }
                }

                HttpContext.Session.SetInt32("IsSearch", 0);    // clear the searching flag
                string userid = _userManager.GetUserId(User);
                try
                {
                    // if this user has a searchView row in the DB, delete it

                    Search searchView = await NoteDataManager.GetUserSearch(_db, userid);

                    if (searchView != null)
                    {
                        _db.Search.Remove(searchView);
                        await _db.SaveChangesAsync();
                    }
                }
                catch
                {
                    // if we cannot talk to the DB, route the user to the setup directions
                    return RedirectToAction("SetUp");
                }

                //Direct link to 3 important files
                ViewData["IFiles"] = await _db.NoteFile
                    .Where(p => p.NoteFileName == "announce" || p.NoteFileName == "pbnotes" || p.NoteFileName == "noteshelp")
                    .OrderBy(p => p.NoteFileName)
                    .ToListAsync();

                // Get a list of all file names for dropdown
                IEnumerable<SelectListItem> items = NoteDataManager.GetFileNameSelectList(_db);
                List<SelectListItem> list2 = new List<SelectListItem>();
                //list2.Add(new SelectListItem
                //{
                //    Value = "",
                //    Text = "-- Select a File --"
                //});
                list2.AddRange(items);

                NFViewModel it = new NFViewModel
                {
                    AFiles = list2
                };

                // Get a list of all file titles for dropdown
                items = NoteDataManager.GetFileTitleSelectList(_db);
                list2 = new List<SelectListItem>();
                //list2.Add(new SelectListItem
                //{
                //    Value = "",
                //    Text = "-- Select a Title --"
                //});
                list2.AddRange(items);
                it.ATitles = list2;
                it.tzone = tzone;

                HomePageMessage mess = await _db.HomePageMessage.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
                it.Message = mess != null ? mess.Message : "";

                return View(_userManager, it);
            }
            catch
            {
                // if we cannot talk to the DB, route the user to the setup directions
                return RedirectToAction("SetUp");
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult Chat(string id, bool id2 = false)
        {
            var options = new PusherOptions
            {
                Encrypted = true
            };
            var pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);

            if (id2)
            {
                var data = new { username = id, message = User.Identity.Name + " wants to chat with you." };
                pusher.Trigger("presence-channel", "chat_request_event", data);
            }
            else
            {
                var data = new { username = User.Identity.Name, message = "<<HAS ARRIVED TO CHAT>>" };
                pusher.Trigger("private-chat-" + id, "show_chat_message_event", data);
            }


            ViewData["ChatFrom"] = User.Identity.Name;  // me
            ViewData["ChatTo"] = id;                    // chat target
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SystemMessage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SystemMessage(SystemMessage x)
        {
            var options = new PusherOptions
            {
                Encrypted = true
            };
            var pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);

            pusher.Trigger("notes-channel", "sys_message_event", new {x.message });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult PusherAuth(string channelName, string socketId)
        {
            if (channelName == null )
                return new OkResult();

            var options = new PusherOptions
            {
                Encrypted = true
            };
            var pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);

            if (!_signInManager.IsSignedIn(User))
                return new UnauthorizedResult();

            if (channelName.StartsWith("private-"))
            {
                var auth = pusher.Authenticate(channelName, socketId);
                var json = auth.ToJson();
                return new ContentResult { Content = json, ContentType = MediaTypeHeaderValue.Parse("application/json").Type };
            }
            if (channelName.StartsWith("presence-"))
            {
                string prefix = Request.IsHttps ? "https://" : "http://";
                var channelData = new PresenceChannelData
                {
                    user_id = _userManager.GetUserId(User),
                    user_info = new
                    {
                        name = User.Identity.Name,
                        host_name = Environment.MachineName,
                        base_url = prefix + Request.Host.Value
                    }
                };

                var auth = pusher.Authenticate(channelName, socketId, channelData);
                var json = auth.ToJson();
                return new ContentResult { Content = json, ContentType = MediaTypeHeaderValue.Parse("application/json").Type };
            }
            // should never happen
            return new OkResult();
        }

        public IActionResult SampleImages()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public IActionResult SetUp()
        {
            return View();
        }

        public IActionResult OpenSource()
        {
            return View();
        }

        public async Task<IActionResult> SetTimeZone()
        {
            TimeZoneModel model = new TimeZoneModel();
            ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));
            model.TimeZoneID = user.TimeZoneID;
            if (model.TimeZoneID < Globals.ZoneMinId)
                model.TimeZoneID = Globals.ZoneUtcid;

            List<TZone> tzones = await _db.TZone.OrderBy(p => p.OffsetHours).ToListAsync();

            model.timeZone = await _db.TZone.SingleAsync(p => p.TZoneID == model.TimeZoneID);

            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "0",
                    Text = "-- Select a Time Zone --"
                }
            };
            foreach (TZone t in tzones)
            {
                list2.Add(new SelectListItem
                {
                    Value = "" + t.TZoneID,
                    Text = t.Offset + " - " + t.Name
                });
            }

            ViewBag.OptionList = list2;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetTimeZone(TimeZoneModel model)
        {
            ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));
            user.TimeZoneID = model.TimeZoneID;
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("TZone");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Preferences()
        {
            ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));

            TextReader sr = new StreamReader(_stylePath);
            ViewBag.DefaultStyle = await sr.ReadToEndAsync();
            sr.Close();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preferences(ApplicationUser model)
        {
            ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));

            user.Pref1 = model.Pref1;
            user.Pref2 = model.Pref2;
            user.MyStyle = model.MyStyle;

            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HomePageMessage()
        {
            List<HomePageMessage> messages = await _db.HomePageMessage.OrderByDescending(p => p.Id).ToListAsync();
            return View(messages);
        }

        public IActionResult Schema()
        {
            return View();
        }

    }
}
