using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_trial.Migrations
{
    /// <inheritdoc />
    public partial class updatedreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Review",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "Idea",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserName",
                table: "Idea",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedDate",
                table: "Idea",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Idea");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserName",
                table: "Idea");

            migrationBuilder.DropColumn(
                name: "ReviewedDate",
                table: "Idea");
        }
    }
}
