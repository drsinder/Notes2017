/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteDataManager.cs
**
**  Description:
**     Note Data Manager methods
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
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notes2017.Services;
using System.Text;
//using Hangfire;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;
using Notes2017.ViewModels;

namespace Notes2017.Models
{
    public static class NoteDataManager
    {
        /// <summary>
        /// Create a NoteFile
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="userId">UserID of creator</param>
        /// <param name="name">NoteFile name</param>
        /// <param name="title">NoteFile title</param>
        /// <returns></returns>
        public static async Task<bool> CreateNoteFile(ApplicationDbContext db, 
            UserManager<ApplicationUser> userManager, 
            string userId, string name, string title)
        {
            var query = db.NoteFile.Where(p => p.NoteFileName == name);
            if (!query.Any())
            {
                NoteFile noteFile = new NoteFile()
                {
                    NoteFileName = name,
                    NoteFileTitle = title,
                    NoteFileID = 0,
                    LastEdited = DateTime.Now.ToUniversalTime()
                };
                db.NoteFile.Add(noteFile);
                await db.SaveChangesAsync();

                NoteFile nf = await db.NoteFile
                    .Where(p => p.NoteFileName == noteFile.NoteFileName)
                    .FirstOrDefaultAsync();

                await AccessManager.CreateBaseEntries(db, userManager, userId, nf.NoteFileID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete a NoteFile
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public static async Task<bool> DeleteNoteFile(ApplicationDbContext db, int id)
        {
            // Things to delete:
            // 1)  X Entries in NoteContent
            // 2)  X Entries in BaseNoteHeader
            // 3)  X Entries in Sequencer
            // 4)  X Entries in NoteAccesses
            // 5)  X Entries in Marks
            // 6)  X Entries in SearchView
            // 7)  1 Entry in NoteFile

            // The above (1 - 6) now done by Cascade Delete of NoteFile

            //List<NoteContent> nc = await _db.NoteContent
            //    .Where(p => p.NoteFileID == id)
            //    .ToListAsync();
            //List<BaseNoteHeader> bnh = await GetBaseNoteHeadersForFile(_db, id);
            //List<Sequencer> seq = await _db.Sequencer
            //.Where(p => p.NoteFileID == id)
            //.ToListAsync();
            //List<NoteAccess> na = await AccessManager.GetAccessListForFile(_db, id);
            //List<Mark> marks = await _db.Mark
            //    .Where(p => p.NoteFileID == id)
            //    .ToListAsync();
            //List<SearchView> sv = await _db.SearchView
            //    .Where(p => p.NoteFileID == id)
            //    .ToListAsync();

            //_db.NoteContent.RemoveRange(nc);
            //_db.BaseNoteHeader.RemoveRange(bnh);
            //_db.Sequencer.RemoveRange(seq);
            //_db.NoteAccess.RemoveRange(na);
            //_db.Mark.RemoveRange(marks);
            //_db.SearchView.RemoveRange(sv);

            List<NoteAccess> na = await AccessManager.GetAccessListForFile(db, id);
            db.NoteAccess.RemoveRange(na);

            List<Subscription> subs = await db.Subscription
                .Where(p => p.NoteFileID == id)
                .ToListAsync();
            db.Subscription.RemoveRange(subs);

            NoteFile noteFile = await db.NoteFile
                .Where(p => p.NoteFileID == id)
                .FirstAsync();
            db.NoteFile.Remove(noteFile);

            await db.SaveChangesAsync();

        return true;
    }

        /// <summary>
        /// Create a Note
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="userManager"></param>
        /// <param name="nc">NoteContent</param>
        /// <param name="send"></param>
        /// <returns></returns>
        public static async Task<NoteContent> CreateNote(ApplicationDbContext db, UserManager<ApplicationUser> userManager, NoteContent nc, bool send)
        {
            if (nc.ResponseOrdinal == 0)  // base note
            {
                nc.NoteOrdinal = await NextBaseNoteOrdinal(db, nc.NoteFileID);
            }

            if (!send) // indicates an import operation / adjust time to UTC / assume original was CST = UTC-06, so add 6 hours
            {
                int offset = 6;
                if (nc.LastEdited.IsDaylightSavingTime())
                    offset--;
                nc.LastEdited = nc.LastEdited.AddHours(offset);
            }

            NoteFile nf = await db.NoteFile
                .Where(p => p.NoteFileID == nc.NoteFileID)
                .FirstOrDefaultAsync();
                
            nf.LastEdited = DateTime.Now.ToUniversalTime();
            db.Entry(nf).State = EntityState.Modified;
            db.NoteContent.Add(nc);
            await db.SaveChangesAsync();

            if (nc.ResponseOrdinal == 0)  // base note
            {
                BaseNoteHeader bnh = new BaseNoteHeader()
                {
                    CreateDate = DateTime.Now.ToUniversalTime(),
                    LastEdited = nc.LastEdited,
                    NoteFileID = nc.NoteFileID,
                    NoteID = nc.NoteID,
                    NoteOrdinal = nc.NoteOrdinal,
                    NoteSubject = nc.NoteSubject,
                    Responses = 0,
                    AuthorName = nc.AuthorName
                };
                if (!string.IsNullOrEmpty(nc.AuthorID))
                    bnh.AuthorID = nc.AuthorID;
                db.BaseNoteHeader.Add(bnh);
                await db.SaveChangesAsync();
            }

            /////

            if (send)
                await SendNewNoteToSubscribers(db, userManager, nc);

            ////

            return nc;
        }

        /// <summary>
        /// Create a Response Note
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="userManager"></param>
        /// <param name="bnh">BaseNoteHeader</param>
        /// <param name="nc">NoteContent</param>
        /// <param name="send"></param>
        /// <returns></returns>
        public static  async Task<NoteContent> CreateResponse(ApplicationDbContext db, UserManager<ApplicationUser> userManager, BaseNoteHeader bnh, NoteContent nc, bool send)
        {
            BaseNoteHeader mine = await GetBaseNoteHeaderById(db, bnh.BaseNoteHeaderID);

            mine.LastEdited = DateTime.Now.ToUniversalTime();
            mine.Responses++;

            db.Entry(mine).State = EntityState.Modified;
            await db.SaveChangesAsync();

            nc.ResponseOrdinal = mine.Responses;
            nc.NoteOrdinal = mine.NoteOrdinal;
            return await CreateNote(db, userManager, nc, send);
        }

        private static async Task SendNewNoteToSubscribers(ApplicationDbContext db, UserManager<ApplicationUser> userManager, NoteContent nc)
        {
            nc.NoteFile.BaseNoteHeaders = null;
            ForwardView fv = new ForwardView()
            {
                NoteSubject = "New Note from Notes 2017"
            };
            List<Subscription> subs = await db.Subscription
                .Where(p => p.NoteFileID == nc.NoteFileID)
                .ToListAsync();

            //List<string> emails = new List<string>();

            fv.ToEmail = "xx";
            fv.FileID = nc.NoteFileID;
            fv.hasstring = false;
            fv.NoteID = nc.NoteID;
            fv.NoteOrdinal = nc.NoteOrdinal;
            fv.NoteSubject = nc.NoteSubject;
            fv.toAllUsers = false;
            fv.IsAdmin = false;
            fv.wholestring = false;


            foreach (Subscription s in subs)
            {
                ApplicationUser usr = await userManager.FindByIdAsync(s.SubscriberID);  //   .ApplicationUser.SingleAsync(p => p.Id == s.SubscriberID);
                NoteAccess myAccess = await AccessManager.GetAccess(db, usr.Id, nc.NoteFileID);

                if (myAccess.Read)
                {
                    //emails.Add(usr.Email);

                    fv.ToEmail = usr.Email;
                    await SendNotesAsync(fv, db, Globals.EmailSender, "BackgroundJob", "Notes 2017");
                }
            }

            //BackgroundJob.Enqueue(() => SendNote(fv, "BackgroundJob", "Notes 2017", nc, emails));
            //BackgroundJob.Enqueue(() => SendNote(fv, "BackgroundJob", "Notes 2017", emails));
        }

        /// <summary>
        /// Gets the Writer Name for a base note 
        /// </summary>
        /// <param name="um">UserManager</param>
        /// <param name="bnh">BaseNoteHeader</param>
        /// <returns></returns>
        public static async Task<string> GetAuthor(UserManager<ApplicationUser> um, BaseNoteHeader bnh)
        {
            if (!string.IsNullOrEmpty(bnh.AuthorID))
            {
                ApplicationUser appuser = await um.FindByIdAsync(bnh.AuthorID);
                return appuser.UserName;
            }
            return bnh.AuthorName;
        }

        /// <summary>
        /// Gets the Writer Name for a note 
        /// </summary>
        /// <param name="um">UserManager</param>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        public static async Task<string> GetAuthor(UserManager<ApplicationUser> um, NoteContent nc)
        {
            if (!string.IsNullOrEmpty(nc.AuthorID))
            {
                ApplicationUser appuser = await um.FindByIdAsync(nc.AuthorID);
                return appuser.UserName;
            }

            return nc.AuthorName;
        }

        /// <summary>
        /// Get next available BaseNoteOrdinal
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="noteFileId">NoteFileID</param>
        /// <returns></returns>
        public static async Task<int> NextBaseNoteOrdinal(ApplicationDbContext db, int noteFileId)
        {
            IOrderedQueryable<BaseNoteHeader> bnhq = GetBaseNoteHeaderByIdRev(db, noteFileId);

            if (bnhq == null || !bnhq.Any())
                return 1;

            BaseNoteHeader bnh = await bnhq.FirstAsync();
            return bnh.NoteOrdinal + 1;
        }

        /// <summary>
        /// Delete a Note
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        public static async Task<bool> DeleteNote(ApplicationDbContext db, NoteContent nc)
        {
            if (nc.ResponseOrdinal == 0)     // base note
            {
                return await DeleteBaseNote(db, nc);
            }
            else  // Response
            {
                return await DeleteResponse(db, nc);
            }
        }

        /// <summary>
        /// Delete a Base Note
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        // Steps involved:
        // 1. Delete all NoteContent rows where NoteFileID, NoteOrdinal match input
        // 2. Delete single row in BaseNoteHeader where NoteFileID, NoteOrdinal match input
        // 3. Decrement all BaseNoteHeader.NoteOrdinal where NoteFileID match input and
        //    BaseNoteHeader.NoteOrdinal > nc.NoteOrdinal
        // 4. Decrement all NoteConent.NoteOrdinal where NoteFileID match input and NoteConent.NoteOrdinal > nc.NoteOrdinal
        public static async Task<bool> DeleteBaseNote(ApplicationDbContext db, NoteContent nc)
        {
            int fileId = nc.NoteFileID;
            int noteOrd = nc.NoteOrdinal;

            try
            {
                BaseNoteHeader deleteBase = await GetBaseNoteHeader(db, fileId, noteOrd);
                List<NoteContent> deleteCont = await GetNoteContentList(db, fileId, noteOrd);

                db.NoteContent.RemoveRange(deleteCont);
                db.BaseNoteHeader.Remove(deleteBase);

                List<BaseNoteHeader> upBase = await db.BaseNoteHeader
                    .Where(p => p.NoteFileID == fileId && p.NoteOrdinal > noteOrd)
                    .ToListAsync();

                foreach (var cont in upBase)
                {
                    cont.NoteOrdinal--;
                    db.Entry(cont).State = EntityState.Modified;
                }

                List<NoteContent> upCont = await db.NoteContent
                    .Where(p => p.NoteFileID == fileId && p.NoteOrdinal > noteOrd)
                    .ToListAsync();

                foreach (var cont in upCont)
                {
                    cont.NoteOrdinal--;
                    db.Entry(cont).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        /// Delete a Response Note
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        // Steps involved:
        // 1. Delete single NoteContent row where NoteFileID, NoteOrdinal, and ResponseOrdinal match input
        // 2. Decrement all NoteConent.ResponseOrdinal where NoteFileID, and NoteOrdinal match input and NoteConent.ResponseOrdinal > nc.ResponseOrdinal
        // 3. Decrement single row (Responses field)in BaseNoteHeader where NoteFileID, NoteOrdinal match input
        public static async Task<bool> DeleteResponse(ApplicationDbContext db, NoteContent nc)
        {
            int fileId = nc.NoteFileID;
            int noteOrd = nc.NoteOrdinal;
            int respOrd = nc.ResponseOrdinal;

            try
            {
                List<NoteContent> deleteCont = await db.NoteContent
                    .Where(p => p.NoteFileID == fileId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal == nc.ResponseOrdinal)
                    .ToListAsync();

                if (deleteCont.Count != 1)
                    return false;
                db.NoteContent.Remove(deleteCont.First());

                List<NoteContent> upCont = await db.NoteContent
                    .Where(p => p.NoteFileID == fileId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal > respOrd)
                    .ToListAsync();

                foreach (var cont in upCont)
                {
                    cont.ResponseOrdinal--;
                    db.Entry(cont).State = EntityState.Modified;
                }

                BaseNoteHeader bnh = await GetBaseNoteHeader(db, fileId, noteOrd);

                bnh.Responses--;
                db.Entry(bnh).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        /// Get the BaseNoteHeader in a given file with given ordinal
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="fileId">fileid</param>
        /// <param name="noteOrd">noteordinal</param>
        /// <returns></returns>
        public static async Task<BaseNoteHeader> GetBaseNoteHeader(ApplicationDbContext db, int fileId, int noteOrd)
        {
            return await db.BaseNoteHeader
                                .Where(p => p.NoteFileID == fileId && p.NoteOrdinal == noteOrd)
                                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get all the BaseNoteHeaders for a file
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="nfid">fileid</param>
        /// <returns></returns>
        public static async Task<List<BaseNoteHeader>> GetBaseNoteHeadersForFile(ApplicationDbContext db, int nfid)
        {
            return await db.BaseNoteHeader
                .Where(p => p.NoteFileID == nfid)
                .OrderBy(p => p.NoteOrdinal)
                .ToListAsync();
        }

        /// <summary>
        /// Get the BaseNoteHeader for a Note
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="nfid">fileid</param>
        /// <param name="noteord"></param>
        /// <returns></returns>
        public static async Task<List<BaseNoteHeader>> GetBaseNoteHeadersForNote(ApplicationDbContext db, int nfid, int noteord)
        {
            return await db.BaseNoteHeader
                .Where(p => p.NoteFileID == nfid && p.NoteOrdinal == noteord)
                .ToListAsync();
        }

        public static async Task<List<NoteContent>> GetBaseNoteAndResponses(ApplicationDbContext db, int nfid, int noteord)
        {
            return await db.NoteContent
                .Where(p => p.NoteFileID == nfid && p.NoteOrdinal == noteord)
                .ToListAsync();
        }

        public static async Task<BaseNoteHeader> GetBaseNoteHeaderById(ApplicationDbContext db, long id)
        {
            return await db.BaseNoteHeader
                .Where(p => p.BaseNoteHeaderID == id)
                .FirstOrDefaultAsync();
        }

        //public static async Task<NoteFile> GetNoteFileByID(ApplicationDbContext db, int id)
        //{
        //    return await db.NoteFile
        //        .Where(p => p.NoteFileID == id)
        //        .FirstOrDefaultAsync();
        //}

        public static async Task<BaseNoteHeader> GetEditedNoteHeader(ApplicationDbContext db, NoteContent edited)
        {
            return await db.BaseNoteHeader
                .Where(p => p.NoteFileID == edited.NoteFileID && p.NoteOrdinal == edited.NoteOrdinal)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<BaseNoteHeader>> GetSbnh(ApplicationDbContext db, Sequencer myseqfile)
        {
            return await db.BaseNoteHeader
                            .Where(x => x.NoteFileID == myseqfile.NoteFileID && x.LastEdited >= myseqfile.LastTime)
                            .OrderBy(x => x.NoteOrdinal)
                            .ToListAsync();
        }

        public static async Task<List<BaseNoteHeader>> GetSearchHeaders(ApplicationDbContext db, Search start, BaseNoteHeader bnh)
        {
            return await db.BaseNoteHeader
                .Where(x => x.NoteFileID == start.NoteFileID && x.NoteOrdinal > bnh.NoteOrdinal)
                .ToListAsync();
        }

        /// <summary>
        /// Get BaseNoteHeaders in reverse order - we only plan to look at the 
        /// first one/one with heighest NoteOrdinal
        /// </summary>
        /// <param name="db"></param>
        /// <param name="noteFileId"></param>
        /// <returns></returns>
        public static IOrderedQueryable<BaseNoteHeader> GetBaseNoteHeaderByIdRev(ApplicationDbContext db, int noteFileId)
        {
            return db.BaseNoteHeader
                            .Where(p => p.NoteFileID == noteFileId)
                            .OrderByDescending(p => p.NoteOrdinal);
        }

        public static async Task<List<BaseNoteHeader>> GetSeqHeader1(ApplicationDbContext db, Sequencer myseqfile, BaseNoteHeader bnh)
        {
            return await db.BaseNoteHeader
                .Where(x => x.NoteFileID == myseqfile.NoteFileID && x.LastEdited >= myseqfile.LastTime && x.NoteOrdinal > bnh.NoteOrdinal)
                .OrderBy(x => x.NoteOrdinal)
                .ToListAsync();
        }

        public static async Task<List<BaseNoteHeader>> GetSeqHeader2(ApplicationDbContext db, Sequencer myseqfile)
        {
            return await db.BaseNoteHeader
                .Where(x => x.NoteFileID == myseqfile.NoteFileID && x.LastEdited >= myseqfile.LastTime)
                .OrderBy(x => x.NoteOrdinal)
                .ToListAsync();
        }

        public static async Task<long> GetNumberOfNotes(ApplicationDbContext db, int fileid)
        {
            List<NoteContent> notes = await db.NoteContent
                                .Where(p => p.NoteFileID == fileid)
                                .ToListAsync();
            return notes.Count;
        }

        public static async Task<long> GetFileLength(ApplicationDbContext db, int fileid)
        {
            NoteFile nf = await GetFileById(db, fileid);
            List<NoteContent> notes = await db.NoteContent
                                .Where(p => p.NoteFileID == fileid)
                                .ToListAsync();

            long lth = 11 + 2 * (nf.NoteFileName.Length + nf.NoteFileTitle.Length);
            foreach(NoteContent nc in notes)
            {
                lth += 2*(nc.NoteBody.Length + nc.NoteSubject.Length + nc.AuthorID.Length + nc.AuthorName.Length) + 17;
                if (!string.IsNullOrEmpty(nc.DirectorMesssage))
                    lth += 2*nc.DirectorMesssage.Length;
                if (nc.ResponseOrdinal == 0)
                {
                    lth += 2*(nc.NoteSubject.Length + nc.AuthorID.Length + nc.AuthorName.Length) + 28;
                }
            }

            return lth;
        }

        /// <summary>
        /// Get a Note by it's ID.
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="noteid">NoteID</param>
        /// <returns>NoteContent</returns>
        public static async Task<NoteContent> GetNoteById(ApplicationDbContext db, long noteid)
        {
            return await db.NoteContent
                .Where(p => p.NoteID == noteid)
                .FirstOrDefaultAsync();
        }

        public static async Task<NoteContent> GetNoteByIdWithFile(ApplicationDbContext db, long noteid)
        {
            NoteContent nc = await db.NoteContent
                .Where(p => p.NoteID == noteid)
                .Include(f => f.NoteFile)
                .FirstOrDefaultAsync();

            return nc;
        }

        /// <summary>
        /// Get a list of all Notes in a string/thread
        /// </summary>
        /// <param name="db">ApplicationDbContext</param>
        /// <param name="fileId">fileid</param>
        /// <param name="noteOrd">NoteOrdinal - identifies the string/thread</param>
        /// <returns></returns>
        public static async Task<List<NoteContent>> GetNoteContentList(ApplicationDbContext db, int fileId, int noteOrd)
        {
            return await db.NoteContent
                .Where(p => p.NoteFileID == fileId && p.NoteOrdinal == noteOrd)
                .ToListAsync();
        }

        public static async Task<List<NoteContent>> GetAllNoteContentList(ApplicationDbContext db, int fileId)
        {
            return await db.NoteContent
                .Where(p => p.NoteFileID == fileId)
                .ToListAsync();
        }


        public static async Task<List<NoteContent>> GetOrderedListOfResponses(ApplicationDbContext db, int nfid, BaseNoteHeader bnh)
        {
            return await db.NoteContent
                .Where(p => p.NoteFileID == nfid && p.NoteOrdinal == bnh.NoteOrdinal && p.ResponseOrdinal > 0)
                .OrderBy(p => p.ResponseOrdinal)
                .ToListAsync();
        }

        public static async Task<NoteContent> GetMarkedNote(ApplicationDbContext db, Mark mark)
        {
            return await db.NoteContent
                .Where(p => p.NoteFileID == mark.NoteFileID && p.NoteOrdinal == mark.NoteOrdinal && p.ResponseOrdinal == mark.ResponseOrdinal)
                .FirstAsync();
        }

        /// <summary>
        /// Given a NoteContent Object and Response number get the response NoteID
        /// </summary>
        /// <param name="db"></param>
        /// <param name="nc"></param>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static async Task<long?> FindResponseId(ApplicationDbContext db, NoteContent nc, int resp)
        {
            NoteContent content = await db.NoteContent
                .Where(p => p.NoteFileID == nc.NoteFileID && p.NoteOrdinal == nc.NoteOrdinal && p.ResponseOrdinal == resp)
                .FirstOrDefaultAsync();

            return content?.NoteID;
        }

        public static async Task<List<NoteContent>> GetSearchResponseList(ApplicationDbContext db, Search start, int myRespOrdinal, BaseNoteHeader bnh)
        {
            // First try responses
            return await db.NoteContent
                .Where(x => x.NoteFileID == start.NoteFileID && x.NoteOrdinal == bnh.NoteOrdinal && x.ResponseOrdinal > myRespOrdinal)
                .ToListAsync();
        }

        public static async Task<NoteFile> GetFileByName(ApplicationDbContext db, string fname)
        {
            return await db.NoteFile
                .Where(p => p.NoteFileName == fname)
                .FirstOrDefaultAsync();
        }

        public static async Task<NoteFile> GetFileById(ApplicationDbContext db, int id)
        {
            return await db.NoteFile
                .Where(p => p.NoteFileID == id)
                .FirstOrDefaultAsync();
        }

        public static async Task<NoteFile> GetFileByIdWithHeaders(ApplicationDbContext db, int id)
        {
            return await db.NoteFile
                .Where(p => p.NoteFileID == id)
                .Include(t => t.BaseNoteHeaders)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<NoteFile>> GetNoteFilesOrderedByName(ApplicationDbContext db)
        {
            return await db.NoteFile
                .OrderBy(p => p.NoteFileName)
                .ToListAsync();
        }

        public static async Task<List<Sequencer>> GetSeqListForUser(ApplicationDbContext db, string userid)
        {
            return await db.Sequencer
                .Where(x => x.UserID == userid)
                .OrderBy(x => x.Ordinal)
                .ToListAsync();
        }

        public static async Task<Search> GetUserSearch(ApplicationDbContext db, string userid)
        {
            return await db.Search
                .Where(p => p.UserID == userid)
                .FirstOrDefaultAsync();
        }

        public static IEnumerable<SelectListItem> GetFileNameSelectList(ApplicationDbContext db)
        {

            // Get a list of all files for dropdowns by name
            return db.NoteFile
                .OrderBy(c => c.NoteFileName)
                .Select(c => new SelectListItem
                {
                    Value = c.NoteFileName,
                    Text = c.NoteFileName
                });
        }

        public static IEnumerable<SelectListItem> GetFileNameSelectListWithId(ApplicationDbContext db)
        {

            // Get a list of all files for dropdowns by name
            return db.NoteFile
                .OrderBy(c => c.NoteFileName)
                .Select(c => new SelectListItem
                {
                    Value = "" + $"{c.NoteFileID}",
                    Text = c.NoteFileName
                });
        }


        public static IEnumerable<SelectListItem> GetFileTitleSelectList(ApplicationDbContext db)
        {

            // Get a list of all files for dropdowns by title
            return db.NoteFile
                .OrderBy(c => c.NoteFileTitle)
                .Select(c => new SelectListItem
                {
                    Value = c.NoteFileName,
                    Text = c.NoteFileTitle
                });
        }


        public static async Task<bool> SendNotesAsync(ForwardView fv, ApplicationDbContext db, IEmailSender emailSender,
            string email, string name)
        {
            NoteContent nc = await GetNoteByIdWithFile(db, fv.NoteID);

            if (!fv.hasstring || !fv.wholestring)
            {
                await emailSender.SendEmailAsync(fv.ToEmail, fv.NoteSubject, "Forwarded by Notes 2017 - User: " + email + " / " + name
                    + "<p>File: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p><hr/>"
                    + "<p>Author: " + nc.AuthorName + "  - Tags: " + nc.DirectorMesssage + "</p><p>"
                    + "<p>Subject: " + nc.NoteSubject + "</p>"
                    + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UTC" + "</p>"
                    + nc.NoteBody
                    + "<hr/>" + "<a href=\"" + Globals.ProductionUrl + "/NoteDisplay/Display/" + fv.NoteID + "\" >Link to note</a>");
            }
            else
            {
                List<BaseNoteHeader> bnhl = await GetBaseNoteHeadersForNote(db, nc.NoteFileID, nc.NoteOrdinal);
                BaseNoteHeader bnh = bnhl[0];
                fv.NoteSubject = bnh.NoteSubject;
                List<NoteContent> notes = await GetBaseNoteAndResponses(db, nc.NoteFileID, nc.NoteOrdinal);

                StringBuilder sb = new StringBuilder();
                sb.Append("Forwarded by Notes 2017 - User: " + email + " / " + name
                    + "<p>\nFile: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p>"
                    + "<hr/>");

                for (int i = 0; i < notes.Count; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("<p>Base Note - " + (notes.Count - 1) + " Response(s)</p>");
                    }
                    else
                    {
                        sb.Append("<hr/><p>Response - " + notes[i].ResponseOrdinal + " of " + (notes.Count - 1) + "</p>");
                    }
                    sb.Append("<p>Author: " + notes[i].AuthorName + "  - Tags: " + notes[i].DirectorMesssage + "</p>");
                    sb.Append("<p>Subject: " + notes[i].NoteSubject + "</p>");
                    sb.Append("<p>" + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UTC" + " </p>");
                    sb.Append(notes[i].NoteBody);
                    sb.Append("<hr/>");
                    sb.Append("<a href=\"");
                    sb.Append(Globals.ProductionUrl + "/NoteDisplay/Display/" + notes[i].NoteID + "\" >Link to note</a>");
                }

                await emailSender.SendEmailAsync(fv.ToEmail, fv.NoteSubject, sb.ToString());
            }

            return true;
        }

        //public static void SendNote(ApplicationDbContext db, ForwardView fv,
        //    string email, string name, NoteContent nc)
        //{
        //    StringBuilder sb = MakeEmail(fv, email, name, nc);
        //    string content = sb.ToString();

        //    Globals.emailSender.SendEmail(fv.ToEmail, fv.NoteSubject, content);

        //    return;
        //}





        // temp comment out
        public static void SendNote(ForwardView fv,
            string email, string name, NoteContent nc, List<string> emails)
        {
            //if (emails.Count < 1)
            //    return;

            //StringBuilder sb = MakeEmail(fv, email, name, nc);
            //string content = sb.ToString();

            //foreach (string item in emails)
            //{
            //    //Globals.emailSender.SendEmail(item, fv.NoteSubject, content);
            //}

            //return;
        }





        //public static void SendNote(ForwardView fv,
        //    string email, string name, List<string> emails)
        //{
        //    if (emails.Count < 1)
        //        return;

        //    // exception here due to DbContext!!

        //    ApplicationDbContext db = new ApplicationDbContext();            

        //    NoteContent nc = db.NoteContent.Single(p => p.NoteFileID == fv.NoteID);

        //    StringBuilder sb = MakeEmail(fv, email, name, nc);
        //    string content = sb.ToString();

        //    foreach (string item in emails)
        //    {
        //        Globals.emailSender.SendEmail(item, fv.NoteSubject, content);
        //    }

        //    return;
        //}


        //    private static StringBuilder MakeEmail(ForwardView fv, string email, string name, NoteContent nc)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append("Forwarded by Notes 2017 - User: ");
        //        sb.Append(email);
        //        sb.Append(" / ");
        //        sb.Append(name);

        //        sb.Append("<p>File: ");
        //        sb.Append(nc.NoteFile.NoteFileName);
        //        sb.Append(" - File Title: ");
        //        sb.Append(nc.NoteFile.NoteFileTitle);

        //        sb.Append("</p><hr/><p>Author: ");
        //        sb.Append(nc.AuthorName);
        //        sb.Append("  - Tags: ");
        //        sb.Append(nc.DirectorMesssage);

        //        sb.Append("</p><hr/><p>Subject: ");
        //        sb.Append(nc.NoteSubject);
        //        sb.Append("</p><p>");

        //        sb.Append(nc.LastEdited.ToShortDateString());
        //        sb.Append(" ");
        //        sb.Append(nc.LastEdited.ToShortTimeString());
        //        sb.Append(" UTC</p>");

        //        sb.Append(nc.NoteBody);

        //        sb.Append("<hr/><a href=\"");
        //        sb.Append(Globals.ProductionUrl);
        //        sb.Append("/NoteDisplay/Display/");
        //        sb.Append(fv.NoteID);
        //        sb.Append("\">Link to note</a>");

        //        return sb;


        ////        string content = "Forwarded by Notes 2017 - User: " + email + " / " + name
        ////+ "<p>File: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p><hr/>"
        ////+ "<p>Author: " + nc.AuthorName + "  - Tags: " + nc.DirectorMesssage + "</p><p>"
        ////+ "<p>Subject: " + nc.NoteSubject + "</p>"
        ////+ nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UTC" + "</p>"
        ////+ nc.NoteBody
        ////+ "<hr/>" + "<a href=\"http:" + Globals.ProductionUrl + "/NoteDisplay/Display/" + fv.NoteID + "\">Link to note</a>";

        //    }

        //    // temp comment out
        //    //public static void SendNotes(ForwardView fv, string email, string name,
        //    //    NoteContent nc, List<BaseNoteHeader> bnhl, List<NoteContent> notes)
        //    //{

        //    //    {
        //    //        BaseNoteHeader bnh = bnhl[0];
        //    //        fv.NoteSubject = bnh.NoteSubject;

        //    //        StringBuilder sb = new StringBuilder();
        //    //        sb.Append("Forwarded by Notes 2017 - User: " + email + " / " + name
        //    //            + "<p>\nFile: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p>"
        //    //            + "<hr/>");

        //    //        for (int i = 0; i < notes.Count; i++)
        //    //        {
        //    //            if (i == 0)
        //    //            {
        //    //                sb.Append("<p>Base Note - " + (notes.Count - 1) + " Response(s)</p>");
        //    //            }
        //    //            else
        //    //            {
        //    //                sb.Append("<hr/><p>Response - " + notes[i].ResponseOrdinal + " of " + (notes.Count - 1) + "</p>");
        //    //            }
        //    //            sb.Append("<p>Author: " + notes[i].AuthorName + "  - Tags: " + notes[i].DirectorMesssage + "</p>");
        //    //            sb.Append("<p>Subject: " + notes[i].NoteSubject + "</p>");
        //    //            sb.Append("<p>" + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UTC" + " </p>");
        //    //            sb.Append(notes[i].NoteBody);
        //    //            sb.Append("<hr/>");
        //    //            sb.Append("<a href=\"");
        //    //            sb.Append(Globals.ProductionUrl + "/NoteDisplay/Display/" + notes[i].NoteID + "\">Link to note</a>");
        //    //        }

        //    //        Globals.emailSender.SendEmail(fv.ToEmail, fv.NoteSubject, sb.ToString());
        //    //    }

        //    //    return;
        //    //}

    }

}
