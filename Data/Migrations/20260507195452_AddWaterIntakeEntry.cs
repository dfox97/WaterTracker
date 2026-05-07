using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWaterIntakeEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WaterIntakeEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    AmountMl = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterIntakeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterIntakeEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WaterIntakeEntries_UserId_RecordedAt",
                table: "WaterIntakeEntries",
                columns: new[] { "UserId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WaterIntakeEntries");
        }
    }
}
