using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PaperlessRESTAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchProcessingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchProcessingHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecordsProcessed = table.Column<int>(type: "integer", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileChecksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchProcessingHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyDocumentAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    AccessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false),
                    SearchCount = table.Column<int>(type: "integer", nullable: false),
                    TotalAccess = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyDocumentAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyDocumentAccesses_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 15, 13, 42, 55, 149, DateTimeKind.Utc).AddTicks(7240));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 15, 13, 42, 55, 149, DateTimeKind.Utc).AddTicks(7240));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 15, 13, 42, 55, 149, DateTimeKind.Utc).AddTicks(7250));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 15, 13, 42, 55, 149, DateTimeKind.Utc).AddTicks(7250));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 15, 13, 42, 55, 149, DateTimeKind.Utc).AddTicks(7250));

            migrationBuilder.CreateIndex(
                name: "IX_BatchProcessingHistories_FileName",
                table: "BatchProcessingHistories",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_BatchProcessingHistories_IsSuccessful",
                table: "BatchProcessingHistories",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_BatchProcessingHistories_ProcessedAt",
                table: "BatchProcessingHistories",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DailyDocumentAccesses_AccessDate",
                table: "DailyDocumentAccesses",
                column: "AccessDate");

            migrationBuilder.CreateIndex(
                name: "IX_DailyDocumentAccesses_DocumentId_AccessDate",
                table: "DailyDocumentAccesses",
                columns: new[] { "DocumentId", "AccessDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchProcessingHistories");

            migrationBuilder.DropTable(
                name: "DailyDocumentAccesses");

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 7, 23, 54, 293, DateTimeKind.Utc).AddTicks(1180));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 7, 23, 54, 293, DateTimeKind.Utc).AddTicks(1180));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 7, 23, 54, 293, DateTimeKind.Utc).AddTicks(1180));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 7, 23, 54, 293, DateTimeKind.Utc).AddTicks(1190));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 7, 23, 54, 293, DateTimeKind.Utc).AddTicks(1190));
        }
    }
}
