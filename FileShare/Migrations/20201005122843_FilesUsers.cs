using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.Migrations
{
    public partial class FilesUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("2dcafeec-8ff0-4d6d-9eab-80ec8a86b0cd"));

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("d65aaa14-ee02-47c9-b83b-2c62a1574715"), new DateTime(2020, 10, 5, 9, 28, 40, 18, DateTimeKind.Local).AddTicks(2975), "application/zip", ".zip" });

            migrationBuilder.CreateIndex(
                name: "IX_FilesUsers_FileId",
                table: "FilesUsers",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FilesUsers_UserId",
                table: "FilesUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilesUsers_Files_FileId",
                table: "FilesUsers",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilesUsers_AspNetUsers_UserId",
                table: "FilesUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilesUsers_Files_FileId",
                table: "FilesUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_FilesUsers_AspNetUsers_UserId",
                table: "FilesUsers");

            migrationBuilder.DropIndex(
                name: "IX_FilesUsers_FileId",
                table: "FilesUsers");

            migrationBuilder.DropIndex(
                name: "IX_FilesUsers_UserId",
                table: "FilesUsers");

            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("d65aaa14-ee02-47c9-b83b-2c62a1574715"));

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("2dcafeec-8ff0-4d6d-9eab-80ec8a86b0cd"), new DateTime(2020, 10, 4, 23, 25, 51, 802, DateTimeKind.Local).AddTicks(7943), "application/zip", ".zip" });
        }
    }
}
