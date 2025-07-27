using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace DevIO.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultRolesSystem : Migration
    {
        private readonly Guid IdGenerated = Guid.Parse("ac0c9d3c-c9f3-4626-809a-41a76edebf2f");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: ["Id", "Name", "NormalizedName", "ConcurrencyStamp"],
                values: [IdGenerated, "Admin", "ADMIN", Guid.NewGuid().ToString()]
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: IdGenerated
            );
        }
    }
}
