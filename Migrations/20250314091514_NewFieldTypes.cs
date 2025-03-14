using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace museia.Migrations
{
    /// <inheritdoc />
    public partial class NewFieldTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostPhoto",
                table: "Post",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostPhoto",
                table: "Post");
        }
    }
}
