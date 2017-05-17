/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: AccessManager.cs
**
**  Description:
**      Access Manager Data Model
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
using Microsoft.AspNetCore.Identity;
using Microsoft.ApplicationInsights;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;

namespace Notes2017.Models
{
    public static class AccessManager
    {
        public static async Task<bool> Create(ApplicationDbContext db, string userId, int noteFileId, bool read, bool respond,
            bool write, bool setTag, bool deleteEdit, bool director, bool editAccess)
        {
            NoteAccess na = new NoteAccess()
            {
                UserID = userId,
                NoteFileID = noteFileId,

                Read = read,
                Respond = respond,
                Write = write,
                SetTag = setTag,
                DeleteEdit = deleteEdit,
                Director = director,
                EditAccess = editAccess
            };
            db.NoteAccess.Add(na);
            return (await db.SaveChangesAsync()) == 1;
        }

        /// <summary>
        /// Create standard starting entires for a access controls for a new file.
        /// "Other" -- no access
        /// creating user (Admin) -- Full Access
        /// readonly@example.com if it exists -- no access
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userManager"></param>
        /// <param name="userId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static async Task<bool> CreateBaseEntries(ApplicationDbContext db, UserManager<ApplicationUser> userManager, string userId, int fileId)
        {
            if (await GetAccess(db, Globals.AccessOther(), fileId) == null)
            {
                bool flag1 = await Create(db, Globals.AccessOther(), fileId, false, false, false, false, false, false, false);
                if (!flag1)
                    return false;
            }

            if (string.Compare(((await GetAccess(db, userId, fileId)).UserID), Globals.AccessOther()) == 0)
            {
                bool flag1 = await Create(db, userId, fileId, true, true, true, true, true, true, true);
                if (!flag1)
                    return false;
            }

            try
            {
                var user = await userManager.FindByNameAsync("readonly@example.com");
                if (user == null)
                    return true;

                string readonlyId = user.Id;

                {
                    bool flag1 = await Create(db, readonlyId, fileId, false, false, false, false, false, false, false);
                    if (!flag1)
                        return false;
                }
            }
            catch
            {
                // ignored
            }
            return true;
        }

        /// <summary>
        /// All access checks call this
        /// </summary>
        /// <param name="db">ApplicationDBContext</param>
        /// <param name="userId">ID of logged in user</param>
        /// <param name="fileId">NoteFileID</param>
        /// <returns>NoteAcess Object</returns>
        public static async Task<NoteAccess> GetAccess(ApplicationDbContext db, string userId, int fileId)
        {
            // First we check for this user specifically
            NoteAccess na = await db.NoteAccess
                .Where(p => p.UserID == userId)
                .Where(p => p.NoteFileID == fileId).FirstOrDefaultAsync();

            if (na != null)
                return na;

            // If specific user not in list use "Other"
            return await db.NoteAccess
                .Where(p => p.UserID == Globals.AccessOther())
                .Where(p => p.NoteFileID == fileId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get Users Specific Access Entry
        /// </summary>
        /// <param name="db">ApplicationDBContext</param>
        /// <param name="userId">ID of logged in user</param>
        /// <param name="fileId">NoteFileID</param>
        /// <returns>NoteAcess Object</returns>
        public static async Task<NoteAccess> GetOneAccess(ApplicationDbContext db, string userId, int fileId)
        {
            NoteAccess na = await db.NoteAccess
                .Where(p => p.UserID == userId)
                .Where(p => p.NoteFileID == fileId).FirstOrDefaultAsync();
            return na;
        }

        public static async Task<List<NoteAccess>> GetAccessListForFile(ApplicationDbContext db, int fileId)
        {
            return await db.NoteAccess
                .Where(p => p.NoteFileID == fileId)
                .ToListAsync();
        }

        public static async Task<bool> Audit(ApplicationDbContext db, string eventType, string userName, string userId,
            string Event, TelemetryClient telemetry)
        {
            Audit na = new Audit();

            var usr = await db.Users.SingleAsync(p => p.UserName == userName);

            na.UserID = usr.Id;
            na.UserName = userName;
            na.EventType = eventType;
            na.Event = Event;
            na.EventTime = System.DateTime.Now.ToUniversalTime();

            telemetry.TrackEvent("Audit - " + userName + " - " + eventType + " - " + Event);

            db.Audit.Add(na);
            return (await db.SaveChangesAsync()) == 1;
        }

    }
}
