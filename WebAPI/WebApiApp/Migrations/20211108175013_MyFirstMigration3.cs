using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApiApp.Migrations
{
    public partial class MyFirstMigration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "captureBy",
                table: "UploadFormDatas",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "captureDate",
                table: "UploadFormDatas",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "captureBy",
                table: "UploadFormDatas");

            migrationBuilder.DropColumn(
                name: "captureDate",
                table: "UploadFormDatas");
        }
    }
}
