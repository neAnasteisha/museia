﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace museia.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBlockedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
            name: "IsBlocked",
            table: "AspNetUsers",
            nullable: false,
            defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "IsBlocked",
            table: "AspNetUsers");
        }
    }
}
