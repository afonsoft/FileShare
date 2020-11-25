using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.Migrations
{
    public partial class LoggerDownloadFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("904d07de-5da7-4a28-8986-c1695637e1bf"));

            migrationBuilder.AddColumn<string>(
                name: "Asn",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsnDomain",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsnName",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsnRoute",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsnType",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallingCode",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContinentCode",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContinentName",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Languages",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Files",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Files",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organisation",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postal",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Files",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LogFileDownload",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    StorageName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreationDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CallingCode = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Postal = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Organisation = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    ContinentCode = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ContinentName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CountryName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RegionCode = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    City = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IP = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Asn = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AsnName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AsnDomain = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AsnRoute = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AsnType = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Languages = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogFileDownload", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("1e81ee22-59fe-4ab5-82b1-50bb3ead4642"), new DateTime(2020, 11, 25, 17, 59, 26, 826, DateTimeKind.Utc).AddTicks(9851), "application/zip", ".zip" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogFileDownload");

            migrationBuilder.DeleteData(
                table: "PermittedExtension",
                keyColumn: "Id",
                keyValue: new Guid("1e81ee22-59fe-4ab5-82b1-50bb3ead4642"));

            migrationBuilder.DropColumn(
                name: "Asn",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AsnDomain",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AsnName",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AsnRoute",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AsnType",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CallingCode",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ContinentCode",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ContinentName",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Organisation",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Postal",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Files");

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension" },
                values: new object[] { new Guid("904d07de-5da7-4a28-8986-c1695637e1bf"), new DateTime(2020, 10, 7, 14, 23, 11, 387, DateTimeKind.Local).AddTicks(2937), "application/zip", ".zip" });
        }
    }
}
