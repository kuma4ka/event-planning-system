using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPlanning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToGuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Guests_EventId",
                table: "Guests");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_EventId_Email",
                table: "Guests",
                columns: new[] { "EventId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Guests_EventId_Email",
                table: "Guests");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_EventId",
                table: "Guests",
                column: "EventId");
        }
    }
}
