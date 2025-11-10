using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class add_some_fields_names : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "positions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "positions",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "positions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "locations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "locations",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "locations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "departments",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "departments",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "departments",
                newName: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "positions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "positions",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "positions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "locations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "locations",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "locations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "departments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "departments",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "departments",
                newName: "CreatedAt");
        }
    }
}
