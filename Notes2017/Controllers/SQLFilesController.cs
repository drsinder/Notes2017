/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: SQLFilesController.cs
**
**  Description:
**      SQL Files Controller for Notes 2017
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
using Microsoft.AspNetCore.Mvc;
using Notes2017.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Net.Http.Headers;
using Notes2017.App_Code;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "User")]
    public class SQLFilesController : NController
    {
        private readonly SQLFileDbContext _sqlcontext;

        public SQLFilesController(IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            SQLFileDbContext sqlcontext) : base(userManager, signInManager)
        {
            _sqlcontext = sqlcontext;
        }


        // GET: SQLFiles/Details/5
        [Authorize(Roles = "User")]
        public async Task<FileResult> GetById(long? id)
        {
            if (id == null)
            {
                //return HttpNotFound();
            }
            SQLFile sQLFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileId == id);
            if (sQLFile == null)
            {
                //return HttpNotFound();
            }


            return File((await (_sqlcontext.SQLFileContent.SingleAsync(m => m.ContentId == sQLFile.ContentID))).Content,
                System.Net.Mime.MediaTypeNames.Application.Octet,
                sQLFile.FileName);
        }


        // GET: SQLFiles/Details/5
        [Authorize(Roles = "User")]
        public async Task<FileResult> GetByName(string id)
        {
            if (id == null)
            {
                //return HttpNotFound();
            }
            SQLFile sQLFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileName == id);
            if (sQLFile == null)
            {
                //return HttpNotFound();
            }


            return File((await (_sqlcontext.SQLFileContent.SingleAsync(m => m.ContentId == sQLFile.ContentID))).Content,
                System.Net.Mime.MediaTypeNames.Application.Octet,
                sQLFile.FileName);
        }


        // GET: SQLFiles
        public async Task<IActionResult> Index()
        {
            return View(await _sqlcontext.SQLFile.ToListAsync());
        }

        // GET: SQLFiles/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SQLFile sQLFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileId == id);
            if (sQLFile == null)
            {
                return NotFound();
            }

            string fileUrl1 = Global.ProductionUrl + "/File/GetById/" + sQLFile.FileId;
            ViewBag.Url1 = fileUrl1;
            string fileUrl2 = Global.ProductionUrl + "/File/GetByName/" + sQLFile.FileName;
            ViewBag.Url2 = fileUrl2;

            return View(sQLFile);
        }

        // GET: SQLFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SQLFiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SQLFile sQLFile, IFormFile file)
        {
            long retId = 0;

            string fname = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

            string nameonly = Path.GetFileNameWithoutExtension(fname);
            string extonly = Path.GetExtension(fname);


            if (true || ModelState.IsValid)
            {
                try
                {
                    if (file.Length > 0 && file.Length < 0x7FFFFFFF )
                    {
                        if (!file.ContentType.StartsWith("image") && !User.IsInRole("Admin"))
                            return RedirectToAction("Index");

                        SQLFileContent content = new SQLFileContent {Content = new byte[file.Length]};
                        using (Stream reader = file.OpenReadStream())
                        {
                            int x = await reader.ReadAsync(content.Content, 0, (int)file.Length);
                            if (x != file.Length)
                                return RedirectToAction("Index");
                        }

                        _sqlcontext.SQLFileContent.Add(content);
                        await _sqlcontext.SaveChangesAsync();
                        retId = content.ContentId;

                        fname = nameonly + "." + retId + extonly;
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception)
                {
                    //string meg = ex.Message;
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

                if (retId < 1)
                    return RedirectToAction("Index");


                sQLFile.ContentID = retId;
                sQLFile.ContentType = file.ContentType;
                sQLFile.Contributor = User.Identity.Name;
                sQLFile.FileName = fname;

                _sqlcontext.SQLFile.Add(sQLFile);
                await _sqlcontext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");

        }

        // GET: SQLFiles/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SQLFile sQLFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileId == id);
            if (sQLFile == null)
            {
                return NotFound();
            }
            return View(sQLFile);
        }

        // POST: SQLFiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SQLFile sQLFile)
        {
            if (ModelState.IsValid)
            {
                _sqlcontext.Update(sQLFile);
                await _sqlcontext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sQLFile);
        }

        // GET: SQLFiles/Delete/5
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SQLFile sQLFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileId == id);
            if (sQLFile == null)
            {
                return NotFound();
            }

            return View(sQLFile);
        }

        // POST: SQLFiles/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            SQLFile sQLFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileId == id);
            SQLFileContent sQLCont = await _sqlcontext.SQLFileContent.SingleAsync(m => m.ContentId == sQLFile.ContentID);

            _sqlcontext.SQLFileContent.Remove(sQLCont);
            _sqlcontext.SQLFile.Remove(sQLFile);
            await _sqlcontext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ThumbNails()
        {
            List<SQLFile> sqlfiles = await _sqlcontext.SQLFile.ToListAsync();
           
            return View(sqlfiles);
        }

    }
}
