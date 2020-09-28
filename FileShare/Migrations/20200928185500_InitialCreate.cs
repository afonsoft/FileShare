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
                    CreationDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermittedExtension", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PermittedExtension",
                columns: new[] { "Id", "CreationDateTime", "Extension" },
                values: new object[] { new Guid("d4bd2f98-4177-4f10-b089-78f665c39140"), new DateTime(2020, 9, 28, 15, 54, 59, 442, DateTimeKind.Local).AddTicks(5122), ".zip" });
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
