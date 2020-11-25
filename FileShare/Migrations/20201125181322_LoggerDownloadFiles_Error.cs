using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.Migrations
{
    public partial class LoggerDownloadFiles_Error : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("1e81ee22-59fe-4ab5-82b1-50bb3ead4642"));

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "LogFileDownload",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("32e6989b-0b23-4e12-a598-4eafa0affd93"), new DateTime(2020, 11, 25, 18, 13, 20, 9, DateTimeKind.Utc).AddTicks(1995), "application/zip", ".zip" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("32e6989b-0b23-4e12-a598-4eafa0affd93"));

            migrationBuilder.DropColumn(
                name: "Error",
                table: "LogFileDownload");

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("1e81ee22-59fe-4ab5-82b1-50bb3ead4642"), new DateTime(2020, 11, 25, 17, 59, 26, 826, DateTimeKind.Utc).AddTicks(9851), "application/zip", ".zip" });
        }
    }
}
