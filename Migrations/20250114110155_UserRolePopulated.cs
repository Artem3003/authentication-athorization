using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace authentication_athorization.Migrations
{
    /// <inheritdoc />
    public partial class UserRolePopulated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "639d303f-7876-4fff-96ec-37f8bd3bf180", "ffaea5d5-0fa5-49f1-9651-fbea4f1d263b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "639d303f-7876-4fff-96ec-37f8bd3bf180", "ffaea5d5-0fa5-49f1-9651-fbea4f1d263b" });
        }
    }
}
