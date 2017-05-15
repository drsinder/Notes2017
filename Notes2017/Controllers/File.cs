/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: FileController.cs
**
**  Description:
**      File Controller for Notes 2017
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
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;

namespace Notes2017.Controllers
{
    public class File : Controller
    {
        //private ApplicationDbContext _context;
        private readonly SQLFileDbContext _sqlcontext;

        public File(ApplicationDbContext context, SQLFileDbContext sqlcontext)
        {
            //_context = context;
            _sqlcontext = sqlcontext;
        }

        public async Task<FileResult> GetById(long? id)
        {
            if (id == null)
                return FileNotFound();

            try
            {
                SQLFile sQlFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileId == id);
                if (sQlFile == null)
                    return FileNotFound();

                return File((await (_sqlcontext.SQLFileContent.SingleAsync(m => m.ContentId == sQlFile.ContentID))).Content,
                    sQlFile.ContentType,
                    sQlFile.FileName);

                //return File((await (_context.SQLFileContent.SingleAsync(m => m.ContentId == sQLFile.ContentID))).Content,
                //    System.Net.Mime.MediaTypeNames.Application.Octet,
                //    sQLFile.FileName);
            }
            catch
            { return FileNotFound(); }
        }

        public async Task<FileResult> GetByName(string id)
        {
            if (id == null)
                return FileNotFound();

            try
            {
                SQLFile sQlFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileName == id);
                if (sQlFile == null)
                    return FileNotFound();

                return File((await (_sqlcontext.SQLFileContent.SingleAsync(m => m.ContentId == sQlFile.ContentID))).Content,
                    sQlFile.ContentType,
                    sQlFile.FileName);
            }
            catch
            { return FileNotFound(); }
        }

        private FileResult FileNotFound()
        {
            return File("~/images/NotFound.png", "image/png");
        }
    }
}
