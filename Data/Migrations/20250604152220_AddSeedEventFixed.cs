using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedEventFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "K-Pop" },
                    { 2, "Rock" },
                    { 3, "EDM" },
                    { 4, "Rap" },
                    { 5, "Ballad" }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "Artists", "Date", "Description", "EndTime", "FacebookUrl", "GenreId", "Location", "MapEmbedUrl", "MediaPath", "ScheduleHtml", "StartTime", "TicketPrice", "Title", "YoutubeUrl" },
                values: new object[] { 1, "BlackPink", new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Buổi biểu diễn đỉnh cao của BlackPink tại Hà Nội!", "22:30", null, 1, "Hà Nội", null, null, null, "18:00", 500000000m, "BlackPink World Tour", null });

            migrationBuilder.CreateIndex(
                name: "IX_Events_GenreId",
                table: "Events",
                column: "GenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Genres_GenreId",
                table: "Events",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Genres_GenreId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Events_GenreId",
                table: "Events");

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
