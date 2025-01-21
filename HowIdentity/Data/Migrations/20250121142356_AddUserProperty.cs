using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HowIdentity.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "exist_in_services",
                table: "users",
                type: "integer[]",
                nullable: false,
                defaultValue: new[] { 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exist_in_services",
                table: "users");
        }
    }
}
