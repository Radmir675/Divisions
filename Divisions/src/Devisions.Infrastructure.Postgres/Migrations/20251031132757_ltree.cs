using Devisions.Domain.Department;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devisions.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Ltree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AddColumn<string>(
                name: "path_temp",
                table: "departments",
                type: "text",
                nullable: false);

            migrationBuilder.Sql(
                @"UPDATE departments
                  SET  path_temp = REPLACE( path,'/', '.')");
            

            migrationBuilder.DropColumn("path", "departments");
            
            migrationBuilder.AddColumn<string>(
                name: "path",
                type: "ltree",
                nullable: false,
                table:"departments");
            
            migrationBuilder.Sql(
                @"UPDATE departments
                  SET path=path_temp::ltree
                  WHERE path_temp IS NOT NULL");
            
            migrationBuilder.DropColumn("temp_path", "departments");

            migrationBuilder.CreateIndex(
                name: "idx_departments_path",
                table: "departments",
                column: "path")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_departments_path",
                table: "departments");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AddColumn<string>(
                name: "path_temp",
                table: "departments",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
            UPDATE departments
            SET path_temp = REPLACE(""path""::text, '.', '/')");

            migrationBuilder.DropColumn(
                name: "path",
                table: "departments");

            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "departments",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE ""Categories"" SET ""Path"" = ""Path_temp""");

            migrationBuilder.DropColumn(
                name: "Path_temp",
                table: "Categories");
        }
    }
}
