using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Notes2017.Data;
using Notes2017.Models;

namespace Notes2017.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Notes2017.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<int>("Ipref2");

                    b.Property<int>("Ipref3");

                    b.Property<int>("Ipref4");

                    b.Property<int>("Ipref5");

                    b.Property<int>("Ipref6");

                    b.Property<int>("Ipref7");

                    b.Property<int>("Ipref8");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("MyStyle")
                        .HasMaxLength(5000);

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<bool>("Pref1");

                    b.Property<bool>("Pref2");

                    b.Property<bool>("Pref3");

                    b.Property<bool>("Pref4");

                    b.Property<bool>("Pref5");

                    b.Property<bool>("Pref6");

                    b.Property<bool>("Pref7");

                    b.Property<bool>("Pref8");

                    b.Property<string>("SecurityStamp");

                    b.Property<int>("TimeZoneID");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Notes2017.Models.Audit", b =>
                {
                    b.Property<long>("AuditID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<DateTime>("EventTime");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<string>("UserID")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.HasKey("AuditID");

                    b.ToTable("Audit");
                });

            modelBuilder.Entity("Notes2017.Models.BaseNoteHeader", b =>
                {
                    b.Property<long>("BaseNoteHeaderID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthorID")
                        .HasMaxLength(450);

                    b.Property<string>("AuthorName")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<DateTime>("CreateDate");

                    b.Property<DateTime>("LastEdited");

                    b.Property<int>("NoteFileID");

                    b.Property<long>("NoteID");

                    b.Property<int>("NoteOrdinal");

                    b.Property<string>("NoteSubject")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<int>("Responses");

                    b.HasKey("BaseNoteHeaderID");

                    b.HasIndex("NoteFileID");

                    b.ToTable("BaseNoteHeader");
                });

            modelBuilder.Entity("Notes2017.Models.HomePageMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<DateTime>("Posted");

                    b.HasKey("Id");

                    b.ToTable("HomePageMessage");
                });

            modelBuilder.Entity("Notes2017.Models.Mark", b =>
                {
                    b.Property<string>("UserID")
                        .HasMaxLength(450);

                    b.Property<int>("NoteFileID");

                    b.Property<int>("MarkOrdinal");

                    b.Property<long>("BaseNoteHeaderID");

                    b.Property<int>("NoteOrdinal");

                    b.Property<int>("ResponseOrdinal");

                    b.HasKey("UserID", "NoteFileID", "MarkOrdinal");

                    b.HasIndex("NoteFileID");

                    b.HasIndex("UserID", "NoteFileID");

                    b.HasIndex("UserID", "NoteFileID", "NoteOrdinal");

                    b.ToTable("Mark");
                });

            modelBuilder.Entity("Notes2017.Models.NoteAccess", b =>
                {
                    b.Property<string>("UserID")
                        .HasMaxLength(450);

                    b.Property<int>("NoteFileID");

                    b.Property<bool>("DeleteEdit");

                    b.Property<bool>("Director");

                    b.Property<bool>("EditAccess");

                    b.Property<bool>("Read");

                    b.Property<bool>("Respond");

                    b.Property<bool>("SetTag");

                    b.Property<bool>("Write");

                    b.HasKey("UserID", "NoteFileID");

                    b.ToTable("NoteAccess");
                });

            modelBuilder.Entity("Notes2017.Models.NoteContent", b =>
                {
                    b.Property<long>("NoteID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthorID")
                        .HasMaxLength(450);

                    b.Property<string>("AuthorName")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("DirectorMesssage")
                        .HasMaxLength(200);

                    b.Property<DateTime>("LastEdited");

                    b.Property<string>("NoteBody")
                        .IsRequired()
                        .HasMaxLength(100000);

                    b.Property<int>("NoteFileID");

                    b.Property<int>("NoteOrdinal");

                    b.Property<string>("NoteSubject")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<int>("ResponseOrdinal");

                    b.HasKey("NoteID");

                    b.HasIndex("NoteFileID");

                    b.ToTable("NoteContent");
                });

            modelBuilder.Entity("Notes2017.Models.NoteFile", b =>
                {
                    b.Property<int>("NoteFileID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("LastEdited");

                    b.Property<string>("NoteFileName")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<string>("NoteFileTitle")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("NoteFileID");

                    b.ToTable("NoteFile");
                });

            modelBuilder.Entity("Notes2017.Models.Search", b =>
                {
                    b.Property<string>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(450);

                    b.Property<int>("BaseOrdinal");

                    b.Property<int>("NoteFileID");

                    b.Property<long>("NoteID");

                    b.Property<int>("Option");

                    b.Property<int>("ResponseOrdinal");

                    b.Property<string>("Text");

                    b.Property<DateTime>("Time");

                    b.HasKey("UserID");

                    b.HasIndex("NoteFileID");

                    b.ToTable("Search");
                });

            modelBuilder.Entity("Notes2017.Models.Sequencer", b =>
                {
                    b.Property<string>("UserID")
                        .HasMaxLength(450);

                    b.Property<int>("NoteFileID");

                    b.Property<bool>("Active");

                    b.Property<DateTime>("LastTime");

                    b.Property<int>("Ordinal");

                    b.Property<DateTime>("StartTime");

                    b.HasKey("UserID", "NoteFileID");

                    b.HasIndex("NoteFileID");

                    b.ToTable("Sequencer");
                });

            modelBuilder.Entity("Notes2017.Models.Subscription", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("NoteFileID");

                    b.Property<string>("SubscriberID")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.HasKey("Id");

                    b.ToTable("Subscription");
                });

            modelBuilder.Entity("Notes2017.Models.TZone", b =>
                {
                    b.Property<int>("TZoneID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Abbreviation")
                        .IsRequired()
                        .HasMaxLength(10);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("Offset")
                        .IsRequired();

                    b.Property<int>("OffsetHours");

                    b.Property<int>("OffsetMinutes");

                    b.HasKey("TZoneID");

                    b.ToTable("TZone");
                });

            modelBuilder.Entity("Notes2017.Models.Words", b =>
                {
                    b.Property<string>("Word")
                        .HasMaxLength(4);

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.Property<int>("ListNum");

                    b.Property<DateTime>("Entered");

                    b.HasKey("Word", "UserName", "ListNum");

                    b.ToTable("Words");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Notes2017.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Notes2017.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Notes2017.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Notes2017.Models.BaseNoteHeader", b =>
                {
                    b.HasOne("Notes2017.Models.NoteFile", "NoteFile")
                        .WithMany("BaseNoteHeaders")
                        .HasForeignKey("NoteFileID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Notes2017.Models.Mark", b =>
                {
                    b.HasOne("Notes2017.Models.NoteFile", "NoteFile")
                        .WithMany()
                        .HasForeignKey("NoteFileID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Notes2017.Models.NoteContent", b =>
                {
                    b.HasOne("Notes2017.Models.NoteFile", "NoteFile")
                        .WithMany()
                        .HasForeignKey("NoteFileID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Notes2017.Models.Search", b =>
                {
                    b.HasOne("Notes2017.Models.NoteFile", "NoteFile")
                        .WithMany()
                        .HasForeignKey("NoteFileID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Notes2017.Models.Sequencer", b =>
                {
                    b.HasOne("Notes2017.Models.NoteFile", "NoteFile")
                        .WithMany()
                        .HasForeignKey("NoteFileID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
