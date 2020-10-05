using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.Migrations
{
    public partial class FileUserModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("df6c8e39-0d29-4a36-bbff-b3eea149d366"));

            migrationBuilder.CreateTable(
                name: "FilesUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreationDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesUsers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("2dcafeec-8ff0-4d6d-9eab-80ec8a86b0cd"), new DateTime(2020, 10, 4, 23, 25, 51, 802, DateTimeKind.Local).AddTicks(7943), "application/zip", ".zip" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilesUsers");

            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("2dcafeec-8ff0-4d6d-9eab-80ec8a86b0cd"));

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("df6c8e39-0d29-4a36-bbff-b3eea149d366"), new DateTime(2020, 10, 4, 23, 6, 51, 229, DateTimeKind.Local).AddTicks(4256), null, ".zip" });
        }
    }
}
