using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event_Managment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddedAdminsTableToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminId);
                });

            migrationBuilder.InsertData(
            table : "Admins",
            columns: new[] { "AdminId", "Name", "Email", "PasswordHash", "ProfileImagePath" },
            values: new object[,] {
                { 1, "Ahmed", "admin1@gmail.com", "AQAAAAIAAYagAAAAEOEx0e4J0V1fyAf+RezgMG50+qoQ7YB9L1TLW1nOuy9qYMzmFnepTo4uVNZinBUg1w==", null },
                { 2, "Bilal", "admin2@gmail.com", "AQAAAAIAAYagAAAAEOEx0e4J0V1fyAf+RezgMG50+qoQ7YB9L1TLW1nOuy9qYMzmFnepTo4uVNZinBUg1w==", null }
            });


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");
        }
    }
}
