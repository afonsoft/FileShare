using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FireShare.Migrations
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
                    Type = table.Column<string>(maxLength: 250, nullable: true),
                    Size = table.Column<long>(nullable: false),
                    IP = table.Column<string>(maxLength: 100, nullable: true),
                    Hash = table.Column<string>(maxLength: 4000, nullable: true),
                    CreationDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");
        }
    }
}
