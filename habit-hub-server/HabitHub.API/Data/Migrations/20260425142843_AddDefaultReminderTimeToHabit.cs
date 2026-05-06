using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultReminderTimeToHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Teams");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DefaultReminderTime",
                table: "Habits",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultReminderTime",
                table: "Habits");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Teams",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
