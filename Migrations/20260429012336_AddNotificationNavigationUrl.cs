using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollabSpace.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationNavigationUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NavigationUrl",
                table: "Notifications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NavigationUrl",
                table: "Notifications");
        }
    }
}
