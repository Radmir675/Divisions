using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devisions.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentIdentifierUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "identify",
                table: "departments",
                newName: "identifier");

            migrationBuilder.CreateIndex(
                name: "IX_departments_identifier",
                table: "departments",
                column: "identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_departments_identifier",
                table: "departments");

            migrationBuilder.RenameColumn(
                name: "identifier",
                table: "departments",
                newName: "identify");
        }
    }
}
