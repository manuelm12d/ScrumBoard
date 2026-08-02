using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScrumBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAtUtc", "Email", "Name", "PasswordHash", "PasswordSalt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@ideasgroup.test", "Admin IdeasGroup", "lbK+fArFcTETf7sxjYqOIbCsAHv7v60EvV3J6KdOkbY=", "tEaLkTcDLCcO9Z5OmbBpDg==" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "dev@ideasgroup.test", "Dev IdeasGroup", "oN1iWW+uYT7u9G0beFtDd7qgkwMgVI6F8lb1DB1wCyY=", "C3tVEoyzsAjrVhjc2Mw7cQ==" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
        }
    }
}
