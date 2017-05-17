/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NController.cs
**
**  Description:
**      Base Controller for other Notes 2017 Controllers
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

using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Notes2017.Models;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Notes2017.Controllers
{
    public class NController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public NController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) : base()
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public override ViewResult View()
        {
            SetStuff();

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View();
        }

        public override ViewResult View(object model)
        {
            SetStuff();

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View(model);
        }

        public override ViewResult View(string viewName)
        {
            SetStuff();

            return base.View(viewName);
        }

        public override ViewResult View(string viewName, object model)
        {
            SetStuff();

            return base.View(viewName, model);
        }

        private void SetStuff()
        {
            ViewBag.UserId = "";
            if (_signInManager.IsSignedIn(User))
            {
                ViewBag.UserId = _userManager.GetUserId(User);
            }

            byte[] b2 = HttpContext.Session.Get("MyStyle");
            if (b2 != null)
            {
                ViewBag.MyStyle = Encoding.ASCII.GetString(b2);
            }
        }

        public ViewResult View(UserManager<ApplicationUser> userManager)
        {
            SetStuff(userManager);

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View();
        }

        public ViewResult View(UserManager<ApplicationUser> userManager, object model)
        {
            SetStuff(userManager);

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View(model);
        }

        public ViewResult View(UserManager<ApplicationUser> userManager, string viewName)
        {
            SetStuff(userManager);

            return base.View(viewName);
        }

        public ViewResult View(UserManager<ApplicationUser> userManager, string viewName, object model)
        {
            SetStuff(userManager);

            return base.View(viewName, model);
        }

        private async void SetStuff(UserManager<ApplicationUser> userManager)
        {
            ViewBag.UserId = "";
            if (_signInManager.IsSignedIn(User))
            {
                ViewBag.UserId = _userManager.GetUserId(User);
            }

            byte[] b2 = HttpContext.Session.Get("MyStyle");
            if (b2 != null)
            {
                ViewBag.MyStyle = Encoding.ASCII.GetString(b2);
            }
            else
            {
                if (_signInManager.IsSignedIn(User))
                {
                    ApplicationUser user = await _userManager.Users.SingleAsync(p => p.Id == _userManager.GetUserId(User));
                    if (!string.IsNullOrEmpty(user.MyStyle))
                    {
                        byte[] b = Encoding.ASCII.GetBytes(user.MyStyle);
                        HttpContext.Session.Set("MyStyle", b);
                    }
                    else
                    {
                        byte[] b = Encoding.ASCII.GetBytes(" ");
                        HttpContext.Session.Set("MyStyle", b);
                    }
                }
            }
        }
    }
}
