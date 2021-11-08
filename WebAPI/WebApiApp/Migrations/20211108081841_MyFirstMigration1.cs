using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApiApp.Migrations
{
    public partial class MyFirstMigration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadFormDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    filename = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    filepath = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    latitude = table.Column<double>(type: "float", nullable: false),
                    longitude = table.Column<double>(type: "float", nullable: false),
                    userName = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    useremail = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    userid = table.Column<string>(type: "nvarchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadFormDatas", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadFormDatas");
        }
    }
}
