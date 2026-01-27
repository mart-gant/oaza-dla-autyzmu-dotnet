using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OazaDlaAutyzmu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FacilityId = table.Column<int>(type: "INTEGER", nullable: false),
                    SenderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SenderEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contact_messages_facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "facility_images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FacilityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMain = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facility_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_facility_images_facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_CreatedAt",
                table: "reviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_FacilityId_IsApproved",
                table: "reviews",
                columns: new[] { "FacilityId", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_IsApproved",
                table: "reviews",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_contact_messages_FacilityId",
                table: "contact_messages",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_messages_IsRead",
                table: "contact_messages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_contact_messages_SentAt",
                table: "contact_messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_facility_images_FacilityId",
                table: "facility_images",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_facility_images_FacilityId_DisplayOrder",
                table: "facility_images",
                columns: new[] { "FacilityId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_facility_images_FacilityId_IsMain",
                table: "facility_images",
                columns: new[] { "FacilityId", "IsMain" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_messages");

            migrationBuilder.DropTable(
                name: "facility_images");

            migrationBuilder.DropIndex(
                name: "IX_reviews_CreatedAt",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_FacilityId_IsApproved",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_IsApproved",
                table: "reviews");
        }
    }
}
