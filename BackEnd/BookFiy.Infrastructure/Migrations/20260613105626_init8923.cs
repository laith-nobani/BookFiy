using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFiy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init8923 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_AspNetUsers_UserId",
                table: "Employee");
            migrationBuilder.AddForeignKey(
                name: "FK_Employee_AspNetUsers_UserId",
                table: "Employee",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Tenant_TenantId",
                table: "Employee");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Tenant_TenantId",
                table: "Employee",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_AspNetUsers_UserId",
                table: "Employee");
            migrationBuilder.AddForeignKey(
                name: "FK_Employee_AspNetUsers_UserId",
                table: "Employee",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // revert Tenant FK to previous behavior (Cascade)
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Tenant_TenantId",
                table: "Employee");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Tenant_TenantId",
                table: "Employee",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
