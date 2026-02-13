using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_trial.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationForeignKeyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdeaId",
                table: "Notification",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewerId",
                table: "Notification",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_IdeaId",
                table: "Notification",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_ReviewerId",
                table: "Notification",
                column: "ReviewerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Idea_IdeaId",
                table: "Notification",
                column: "IdeaId",
                principalTable: "Idea",
                principalColumn: "IdeaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_User_ReviewerId",
                table: "Notification",
                column: "ReviewerId",
                principalTable: "User",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Idea_IdeaId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_User_ReviewerId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_IdeaId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_ReviewerId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "IdeaId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "Notification");
        }
    }
}
