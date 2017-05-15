using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Notes2017.Data;

namespace Notes2017.Migrations
{
    [DbContext(typeof(SQLFileDbContext))]
    [Migration("20170316095525_FileTables")]
    partial class FileTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Notes2017.Models.SQLFile", b =>
                {
                    b.Property<long>("FileId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comments")
                        .HasMaxLength(1000);

                    b.Property<long>("ContentID");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<string>("Contributor")
                        .IsRequired()
                        .HasMaxLength(300);

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(300);

                    b.HasKey("FileId");

                    b.HasIndex("FileName");

                    b.ToTable("SQLFile");
                });

            modelBuilder.Entity("Notes2017.Models.SQLFileContent", b =>
                {
                    b.Property<long>("ContentId")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Content")
                        .IsRequired();

                    b.HasKey("ContentId");

                    b.ToTable("SQLFileContent");
                });
        }
    }
}
