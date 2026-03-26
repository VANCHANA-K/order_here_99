using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QrFoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTraceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TraceId",
                table: "AuditLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TraceId",
                table: "AuditLogs");
        }
    }
}
