using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GetAccredited.Migrations.ApplicationDb
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    CustomerId = table.Column<string>(nullable: false),
                    IsCredit = table.Column<bool>(nullable: false),
                    Holder = table.Column<string>(nullable: true),
                    Number = table.Column<string>(nullable: true),
                    Expiry = table.Column<DateTime>(nullable: false),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Acronym = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    WebsiteUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.CreateTable(
                name: "Accreditations",
                columns: table => new
                {
                    AccreditationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    CreatorId = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Eligibility = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accreditations", x => x.AccreditationId);
                    table.ForeignKey(
                        name: "FK_Accreditations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<string>(nullable: true),
                    StudentId = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(nullable: true),
                    AppointmentId = table.Column<int>(nullable: true),
                    AccreditationId = table.Column<int>(nullable: true),
                    DateBooked = table.Column<DateTime>(nullable: false),
                    IsCancelled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Accreditations_AccreditationId",
                        column: x => x.AccreditationId,
                        principalTable: "Accreditations",
                        principalColumn: "AccreditationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(nullable: true),
                    NewAppointmentAppointmentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Requests_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Appointments_NewAppointmentAppointmentId",
                        column: x => x.NewAppointmentAppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accreditations_OrganizationId",
                table: "Accreditations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_OrganizationId",
                table: "Appointments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AccreditationId",
                table: "Bookings",
                column: "AccreditationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AppointmentId",
                table: "Bookings",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_BookingId",
                table: "Requests",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_NewAppointmentAppointmentId",
                table: "Requests",
                column: "NewAppointmentAppointmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Accreditations");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
