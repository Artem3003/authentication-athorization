using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace authentication_athorization.Migrations
{
    /// <inheritdoc />
    public partial class EncryptedSecretKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnctyptedSecretKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnctyptedSecretKey",
                table: "AspNetUsers");
        }
    }
}
