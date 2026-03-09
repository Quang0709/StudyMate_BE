using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileAndRemoveTeacherBio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bio",
                table: "teachers");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bio",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "birthday",
                table: "users",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "bio",
                table: "users");

            migrationBuilder.DropColumn(
                name: "birthday",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "bio",
                table: "teachers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
