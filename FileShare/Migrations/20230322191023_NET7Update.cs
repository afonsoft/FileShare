using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileShare.Migrations
{
    /// <inheritdoc />
    public partial class NET7Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("32e6989b-0b23-4e12-a598-4eafa0affd93"));

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("fd5a1a81-5b37-4ebc-a2ee-d9ba7f585c6e"), new DateTime(2023, 3, 22, 19, 10, 23, 126, DateTimeKind.Utc).AddTicks(4277), "application/zip", ".zip" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("fd5a1a81-5b37-4ebc-a2ee-d9ba7f585c6e"));

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("32e6989b-0b23-4e12-a598-4eafa0affd93"), new DateTime(2020, 11, 25, 18, 13, 20, 9, DateTimeKind.Utc).AddTicks(1995), "application/zip", ".zip" });
        }
    }
}
