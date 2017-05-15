/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: ApplicationDbContext.js
**
**  Description:
**      Main Application Db Context
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notes2017.Models;

namespace Notes2017.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<BaseNoteHeader> BaseNoteHeader { get; set; }
        public DbSet<NoteAccess> NoteAccess { get; set; }
        public DbSet<NoteContent> NoteContent { get; set; }
        public DbSet<NoteFile> NoteFile { get; set; }
        public DbSet<Sequencer> Sequencer { get; set; }
        public DbSet<Words> Words { get; set; }
        public DbSet<Search> Search { get; set; }
        public DbSet<Mark> Mark { get; set; }
        public DbSet<Audit> Audit { get; set; }
        public DbSet<TZone> TZone { get; set; }
        public DbSet<HomePageMessage> HomePageMessage { get; set; }
        public DbSet<Subscription> Subscription { get; set; }

        //public DbSet<EndToEnd> EndToEnd { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<NoteAccess>()
    .HasKey(new string[] { "UserID", "NoteFileID" });

            builder.Entity<Sequencer>()
                .HasKey(new string[] { "UserID", "NoteFileID" });

            builder.Entity<Words>()
                .HasKey(new string[] { "Word", "UserName", "ListNum" });

            builder.Entity<Search>()
                .HasKey(new string[] { "UserID" });

            builder.Entity<Mark>()
                .HasKey(new string[] { "UserID", "NoteFileID", "MarkOrdinal" });

            builder.Entity<Mark>()
                .HasIndex(new string[] { "UserID", "NoteFileID" });

            builder.Entity<Mark>()
                .HasIndex(new string[] { "UserID", "NoteFileID", "NoteOrdinal" });


            //builder.Entity<EndToEnd>()
            //    .HasIndex(new string[] { "Word0", "Word1", "Word2", "Word3",
            //        "Word4", "Word5", "Word6", "Word7", "Word8", "Word9",
            //        "Word10", "UserName", "ListNum" });


        }
    }
}
