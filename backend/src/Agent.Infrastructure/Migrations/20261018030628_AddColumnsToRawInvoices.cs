using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToRawInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "Amount", table: "RawInvoices", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<bool>(name: "IsDuplicate", table: "RawInvoices", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsIllegal", table: "RawInvoices", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "NeedsReview", table: "RawInvoices", type: "boolean", nullable: false, defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Amount", table: "RawInvoices");
            migrationBuilder.DropColumn(name: "IsDuplicate", table: "RawInvoices");
            migrationBuilder.DropColumn(name: "IsIllegal", table: "RawInvoices");
            migrationBuilder.DropColumn(name: "NeedsReview", table: "RawInvoices");
        }
    }
}