using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance_Tracker.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarColorToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarColor",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarColor",
                table: "AspNetUsers");
        }
    }
}
