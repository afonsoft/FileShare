using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.Migrations
{
    public partial class ChangeMaxLengthExtension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("f3bffe60-636d-4855-b93b-b647d23632fa"));

            migrationBuilder.AlterColumn<string>(
                name: "Extension",
                table: "PermittedExtension",
                maxLength: 18,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PermittedExtension",
                maxLength: 350,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("904d07de-5da7-4a28-8986-c1695637e1bf"), new DateTime(2020, 10, 7, 14, 23, 11, 387, DateTimeKind.Local).AddTicks(2937), "application/zip", ".zip" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("904d07de-5da7-4a28-8986-c1695637e1bf"));

            migrationBuilder.AlterColumn<string>(
                name: "Extension",
                table: "PermittedExtension",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PermittedExtension",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 350,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("f3bffe60-636d-4855-b93b-b647d23632fa"), new DateTime(2020, 10, 5, 9, 31, 9, 850, DateTimeKind.Local).AddTicks(4874), "application/zip", ".zip" });
        }
    }
}
