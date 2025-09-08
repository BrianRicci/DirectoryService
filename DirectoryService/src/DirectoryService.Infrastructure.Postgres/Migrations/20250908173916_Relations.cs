using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Relations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_DepartmentId1",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_departments_DepartmentId1",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_DepartmentId1",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_DepartmentId1",
                table: "department_locations");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "department_positions");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "department_positions",
                newName: "position_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "department_positions",
                newName: "department_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "department_locations",
                newName: "location_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "department_locations",
                newName: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_position_id",
                table: "department_positions",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_location_id",
                table: "department_locations",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_departments_department_id",
                table: "department_positions",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions",
                column: "position_id",
                principalTable: "positions",
                principalColumn: "position_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_departments_department_id",
                table: "department_positions");

            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_location_id",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "department_positions",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "department_positions",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "department_locations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "department_locations",
                newName: "DepartmentId");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId1",
                table: "department_positions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId1",
                table: "department_locations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_DepartmentId1",
                table: "department_positions",
                column: "DepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_DepartmentId1",
                table: "department_locations",
                column: "DepartmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_DepartmentId1",
                table: "department_locations",
                column: "DepartmentId1",
                principalTable: "departments",
                principalColumn: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_departments_DepartmentId1",
                table: "department_positions",
                column: "DepartmentId1",
                principalTable: "departments",
                principalColumn: "department_id");
        }
    }
}
