using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordPoliciesDemo.API.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedOn = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(nullable: true),
                    FirstName = table.Column<string>(maxLength: 150, nullable: true),
                    LastName = table.Column<string>(maxLength: 150, nullable: true),
                    Password = table.Column<string>(nullable: true),
                    LastPasswordChangedDate = table.Column<DateTimeOffset>(nullable: true),
                    Username = table.Column<string>(maxLength: 100, nullable: true),
                    LastLogin = table.Column<DateTimeOffset>(nullable: true),
                    FailedLoginAttempts = table.Column<int>(nullable: false),
                    CannotLoginUntil = table.Column<DateTimeOffset>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_FirstName",
                table: "ApplicationUsers",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_LastName",
                table: "ApplicationUsers",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_Username",
                table: "ApplicationUsers",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUsers");
        }
    }
}
