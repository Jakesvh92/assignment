using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApiApp.Migrations
{
    public partial class MyFirstMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imgtype",
                table: "UploadFormDatas",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "UploadFormDatas",
                type: "nvarchar(500)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imgtype",
                table: "UploadFormDatas");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "UploadFormDatas");
        }
    }
}
