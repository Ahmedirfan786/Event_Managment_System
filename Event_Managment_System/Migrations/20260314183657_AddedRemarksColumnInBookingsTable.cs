using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event_Managment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddedRemarksColumnInBookingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "Bookings");
        }
    }
}
