using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Notes2017.Data.Migrations
{
    public partial class UserTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AddColumn<int>(
                name: "Ipref2",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref3",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref4",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref5",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref6",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref7",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref8",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MyStyle",
                table: "AspNetUsers",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Pref1",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref2",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref3",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref4",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref5",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref6",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref7",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref8",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimeZoneID",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Audit",
                columns: table => new
                {
                    AuditID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Event = table.Column<string>(maxLength: 1000, nullable: false),
                    EventTime = table.Column<DateTime>(nullable: false),
                    EventType = table.Column<string>(maxLength: 20, nullable: false),
                    UserID = table.Column<string>(maxLength: 450, nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit", x => x.AuditID);
                });

            migrationBuilder.CreateTable(
                name: "HomePageMessage",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(maxLength: 1000, nullable: false),
                    Posted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePageMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoteAccess",
                columns: table => new
                {
                    UserID = table.Column<string>(maxLength: 450, nullable: false),
                    NoteFileID = table.Column<int>(nullable: false),
                    DeleteEdit = table.Column<bool>(nullable: false),
                    Director = table.Column<bool>(nullable: false),
                    EditAccess = table.Column<bool>(nullable: false),
                    Read = table.Column<bool>(nullable: false),
                    Respond = table.Column<bool>(nullable: false),
                    SetTag = table.Column<bool>(nullable: false),
                    Write = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteAccess", x => new { x.UserID, x.NoteFileID });
                });

            migrationBuilder.CreateTable(
                name: "NoteFile",
                columns: table => new
                {
                    NoteFileID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LastEdited = table.Column<DateTime>(nullable: false),
                    NoteFileName = table.Column<string>(maxLength: 20, nullable: false),
                    NoteFileTitle = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteFile", x => x.NoteFileID);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NoteFileID = table.Column<int>(nullable: false),
                    SubscriberID = table.Column<string>(maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TZone",
                columns: table => new
                {
                    TZoneID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Abbreviation = table.Column<string>(maxLength: 10, nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Offset = table.Column<string>(nullable: false),
                    OffsetHours = table.Column<int>(nullable: false),
                    OffsetMinutes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TZone", x => x.TZoneID);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Word = table.Column<string>(maxLength: 4, nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: false),
                    ListNum = table.Column<int>(nullable: false),
                    Entered = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => new { x.Word, x.UserName, x.ListNum });
                });

            migrationBuilder.CreateTable(
                name: "BaseNoteHeader",
                columns: table => new
                {
                    BaseNoteHeaderID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthorID = table.Column<string>(maxLength: 450, nullable: true),
                    AuthorName = table.Column<string>(maxLength: 256, nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    LastEdited = table.Column<DateTime>(nullable: false),
                    NoteFileID = table.Column<int>(nullable: false),
                    NoteID = table.Column<long>(nullable: false),
                    NoteOrdinal = table.Column<int>(nullable: false),
                    NoteSubject = table.Column<string>(maxLength: 200, nullable: false),
                    Responses = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseNoteHeader", x => x.BaseNoteHeaderID);
                    table.ForeignKey(
                        name: "FK_BaseNoteHeader_NoteFile_NoteFileID",
                        column: x => x.NoteFileID,
                        principalTable: "NoteFile",
                        principalColumn: "NoteFileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mark",
                columns: table => new
                {
                    UserID = table.Column<string>(maxLength: 450, nullable: false),
                    NoteFileID = table.Column<int>(nullable: false),
                    MarkOrdinal = table.Column<int>(nullable: false),
                    BaseNoteHeaderID = table.Column<long>(nullable: false),
                    NoteOrdinal = table.Column<int>(nullable: false),
                    ResponseOrdinal = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mark", x => new { x.UserID, x.NoteFileID, x.MarkOrdinal });
                    table.ForeignKey(
                        name: "FK_Mark_NoteFile_NoteFileID",
                        column: x => x.NoteFileID,
                        principalTable: "NoteFile",
                        principalColumn: "NoteFileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoteContent",
                columns: table => new
                {
                    NoteID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthorID = table.Column<string>(maxLength: 450, nullable: true),
                    AuthorName = table.Column<string>(maxLength: 256, nullable: false),
                    DirectorMesssage = table.Column<string>(maxLength: 200, nullable: true),
                    LastEdited = table.Column<DateTime>(nullable: false),
                    NoteBody = table.Column<string>(maxLength: 100000, nullable: false),
                    NoteFileID = table.Column<int>(nullable: false),
                    NoteOrdinal = table.Column<int>(nullable: false),
                    NoteSubject = table.Column<string>(maxLength: 200, nullable: false),
                    ResponseOrdinal = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteContent", x => x.NoteID);
                    table.ForeignKey(
                        name: "FK_NoteContent_NoteFile_NoteFileID",
                        column: x => x.NoteFileID,
                        principalTable: "NoteFile",
                        principalColumn: "NoteFileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Search",
                columns: table => new
                {
                    UserID = table.Column<string>(maxLength: 450, nullable: false),
                    BaseOrdinal = table.Column<int>(nullable: false),
                    NoteFileID = table.Column<int>(nullable: false),
                    NoteID = table.Column<long>(nullable: false),
                    Option = table.Column<int>(nullable: false),
                    ResponseOrdinal = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Search", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Search_NoteFile_NoteFileID",
                        column: x => x.NoteFileID,
                        principalTable: "NoteFile",
                        principalColumn: "NoteFileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sequencer",
                columns: table => new
                {
                    UserID = table.Column<string>(maxLength: 450, nullable: false),
                    NoteFileID = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    LastTime = table.Column<DateTime>(nullable: false),
                    Ordinal = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sequencer", x => new { x.UserID, x.NoteFileID });
                    table.ForeignKey(
                        name: "FK_Sequencer_NoteFile_NoteFileID",
                        column: x => x.NoteFileID,
                        principalTable: "NoteFile",
                        principalColumn: "NoteFileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseNoteHeader_NoteFileID",
                table: "BaseNoteHeader",
                column: "NoteFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_NoteFileID",
                table: "Mark",
                column: "NoteFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_UserID_NoteFileID",
                table: "Mark",
                columns: new[] { "UserID", "NoteFileID" });

            migrationBuilder.CreateIndex(
                name: "IX_Mark_UserID_NoteFileID_NoteOrdinal",
                table: "Mark",
                columns: new[] { "UserID", "NoteFileID", "NoteOrdinal" });

            migrationBuilder.CreateIndex(
                name: "IX_NoteContent_NoteFileID",
                table: "NoteContent",
                column: "NoteFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Search_NoteFileID",
                table: "Search",
                column: "NoteFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Sequencer_NoteFileID",
                table: "Sequencer",
                column: "NoteFileID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit");

            migrationBuilder.DropTable(
                name: "BaseNoteHeader");

            migrationBuilder.DropTable(
                name: "HomePageMessage");

            migrationBuilder.DropTable(
                name: "Mark");

            migrationBuilder.DropTable(
                name: "NoteAccess");

            migrationBuilder.DropTable(
                name: "NoteContent");

            migrationBuilder.DropTable(
                name: "Search");

            migrationBuilder.DropTable(
                name: "Sequencer");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "TZone");

            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "NoteFile");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Ipref2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref3",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref4",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref5",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref6",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref7",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref8",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MyStyle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref3",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref4",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref5",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref6",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref7",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref8",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TimeZoneID",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");
        }
    }
}
