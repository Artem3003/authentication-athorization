using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace authentication_athorization.Migrations
{
    /// <inheritdoc />
    public partial class InitialRoleSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Desctiption",
                table: "AspNetRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Desctiption", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "45deb9d6-c1ae-44a6-8b3b-3b3b3b3b3b3b", null, "The admin role for the user.", "Admin", "ADMIN" },
                    { "639d303f-7876-4fff-96ec-37f8bd3bf180", null, "The visitor role for the user.", "Visitor", "VISITOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "45deb9d6-c1ae-44a6-8b3b-3b3b3b3b3b3b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "639d303f-7876-4fff-96ec-37f8bd3bf180");

            migrationBuilder.DropColumn(
                name: "Desctiption",
                table: "AspNetRoles");
        }
    }
}
