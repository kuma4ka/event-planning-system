using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPlanning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDoubleOptIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "NewsletterSubscribers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "NewsletterSubscribers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "NewsletterSubscribers");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "NewsletterSubscribers");
        }
    }
}
