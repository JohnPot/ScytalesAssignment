using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "driving_licence_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    licence_category = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    photo = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 500, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driving_licence_applications", x => x.id);
                    table.CheckConstraint("chk_status", "status IN (\r\n                    'Pending',\r\n                    'Submitted',\r\n                    'ValidatingData',\r\n                    'CrossCheckingRegistry',\r\n                    'CheckingPhoto',\r\n                    'RiskAssessment',\r\n                    'PendingManualReview',\r\n                    'Approved',\r\n                    'Rejected',\r\n                    'Failed'\r\n                )");
                });

            migrationBuilder.CreateTable(
                name: "outbox_event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_event", x => x.id);
                    table.CheckConstraint("chk_status", "status IN (\r\n                    'Pending',\r\n                    'Submitted'\r\n                )");
                });

            migrationBuilder.CreateTable(
                name: "processed_event",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "application_audits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", maxLength: 500, nullable: false),
                    from_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    to_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_audits", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_audits_driving_licence_applications_application~",
                        column: x => x.application_id,
                        principalTable: "driving_licence_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_application_audits_application_id",
                table: "application_audits",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "uq_national_id",
                table: "driving_licence_applications",
                column: "national_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_applications_email",
                table: "driving_licence_applications",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_event_status_created_at",
                table: "outbox_event",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_processed_event_Id",
                table: "processed_event",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_processed_event_IdempotencyKey",
                table: "processed_event",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_audits");

            migrationBuilder.DropTable(
                name: "outbox_event");

            migrationBuilder.DropTable(
                name: "processed_event");

            migrationBuilder.DropTable(
                name: "driving_licence_applications");
        }
    }
}
