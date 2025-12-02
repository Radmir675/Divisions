using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devisions.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "version",
                table: "departments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                table: "departments");
        }
    }
}
