using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Notes2017.Migrations
{
    public partial class FileTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SQLFile",
                columns: table => new
                {
                    FileId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Comments = table.Column<string>(maxLength: 1000, nullable: true),
                    ContentID = table.Column<long>(nullable: false),
                    ContentType = table.Column<string>(maxLength: 100, nullable: false),
                    Contributor = table.Column<string>(maxLength: 300, nullable: false),
                    FileName = table.Column<string>(maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SQLFile", x => x.FileId);
                });

            migrationBuilder.CreateTable(
                name: "SQLFileContent",
                columns: table => new
                {
                    ContentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SQLFileContent", x => x.ContentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SQLFile_FileName",
                table: "SQLFile",
                column: "FileName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SQLFile");

            migrationBuilder.DropTable(
                name: "SQLFileContent");
        }
    }
}
