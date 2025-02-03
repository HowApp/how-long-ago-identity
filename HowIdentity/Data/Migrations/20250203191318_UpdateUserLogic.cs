using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HowIdentity.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exist_in_services",
                table: "users");

            migrationBuilder.CreateTable(
                name: "user_microservices",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    micro_service = table.Column<int>(type: "integer", nullable: false),
                    confirm_existing = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_microservices", x => new { x.user_id, x.micro_service });
                    table.ForeignKey(
                        name: "fk_user_microservices_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_microservices_confirm_existing",
                table: "user_microservices",
                column: "confirm_existing");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_microservices");

            migrationBuilder.AddColumn<int[]>(
                name: "exist_in_services",
                table: "users",
                type: "integer[]",
                nullable: false,
                defaultValue: new[] { 1 });
        }
    }
}
