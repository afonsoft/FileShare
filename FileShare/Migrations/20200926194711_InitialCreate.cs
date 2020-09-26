using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 400, nullable: true),
                    StorageName = table.Column<string>(maxLength: 400, nullable: true),
                    Type = table.Column<string>(maxLength: 200, nullable: true),
                    Size = table.Column<long>(nullable: false),
                    IP = table.Column<string>(maxLength: 100, nullable: true),
                    Hash = table.Column<string>(maxLength: 4000, nullable: true),
                    CreationDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermittedExtension",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Extension = table.Column<string>(maxLength: 10, nullable: true),
                    Description = table.Column<string>(maxLength: 400, nullable: true),
                    MimeType = table.Column<string>(maxLength: 200, nullable: true, defaultValue: "application/octet-stream"),
                    CreationDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermittedExtension", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Description", "Extension", "MimeType" },
                values: new object[] { new Guid("b430f084-9f68-4d2a-b2a9-fc2c4c422a83"), new DateTime(2020, 9, 26, 16, 47, 11, 196, DateTimeKind.Local).AddTicks(5026), null, ".zip", "application/zip" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "PermittedExtension");
        }
    }
}
