using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace museia.Migrations
{
    /// <inheritdoc />
    public partial class AddCounterToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountOfWarnings",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountOfWarnings",
                table: "AspNetUsers");
        }
    }
}
