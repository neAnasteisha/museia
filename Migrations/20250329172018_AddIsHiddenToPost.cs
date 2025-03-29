using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace museia.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Post",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Post");
        }
    }
}
