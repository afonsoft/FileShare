using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.Migrations
{
    public partial class FilesUsersForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("d65aaa14-ee02-47c9-b83b-2c62a1574715"));

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("f3bffe60-636d-4855-b93b-b647d23632fa"), new DateTime(2020, 10, 5, 9, 31, 9, 850, DateTimeKind.Local).AddTicks(4874), "application/zip", ".zip" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("f3bffe60-636d-4855-b93b-b647d23632fa"));

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("d65aaa14-ee02-47c9-b83b-2c62a1574715"), new DateTime(2020, 10, 5, 9, 28, 40, 18, DateTimeKind.Local).AddTicks(2975), "application/zip", ".zip" });
        }
    }
}
