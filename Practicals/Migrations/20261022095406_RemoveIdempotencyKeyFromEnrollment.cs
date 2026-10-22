using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practicals.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdempotencyKeyFromEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_IdempotencyKey",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Enrollments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Enrollments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_IdempotencyKey",
                table: "Enrollments",
                column: "IdempotencyKey",
                unique: true);
        }
    }
}
