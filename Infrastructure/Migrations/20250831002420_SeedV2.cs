using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_DentistId_StartUtc",
                table: "Appointments");

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.InsertData(
                table: "Dentists",
                columns: new[] { "Id", "FullName" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-222222222222"), "Dr. Bob Lee" },
                    { new Guid("11111111-1111-1111-1111-333333333333"), "Dr. Carol Tan" }
                });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Email", "FullName" },
                values: new object[] { "jane.roe@example.com", "Jane Roe" });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Email", "FullName" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-111111111111"), "john@example.com", "John Doe" },
                    { new Guid("22222222-2222-2222-2222-333333333333"), "omar.aziz@example.com", "Omar Aziz" },
                    { new Guid("22222222-2222-2222-2222-444444444444"), "lina.haddad@example.com", "Lina Haddad" },
                    { new Guid("22222222-2222-2222-2222-555555555555"), "mei.chen@example.com", "Mei Chen" },
                    { new Guid("22222222-2222-2222-2222-666666666666"), "carlos.ruiz@example.com", "Carlos Ruiz" }
                });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "DurationMinutes", "Name" },
                values: new object[] { 45, "Whitening" });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "DurationMinutes", "Name" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-111111111111"), 30, "Cleaning" },
                    { new Guid("33333333-3333-3333-3333-222222222222"), 60, "Filling" }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "DentistId", "DurationMinutes", "PatientId", "ServiceId", "StartUtc", "Status" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-111111111111"), new Guid("11111111-1111-1111-1111-111111111111"), 30, new Guid("22222222-2222-2222-2222-111111111111"), new Guid("33333333-3333-3333-3333-111111111111"), new DateTimeOffset(new DateTime(2025, 9, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0 },
                    { new Guid("44444444-4444-4444-4444-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), 30, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-111111111111"), new DateTimeOffset(new DateTime(2025, 9, 1, 9, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0 },
                    { new Guid("44444444-4444-4444-4444-333333333333"), new Guid("11111111-1111-1111-1111-111111111111"), 60, new Guid("22222222-2222-2222-2222-333333333333"), new Guid("33333333-3333-3333-3333-222222222222"), new DateTimeOffset(new DateTime(2025, 9, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("11111111-1111-1111-1111-222222222222"), 45, new Guid("22222222-2222-2222-2222-444444444444"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTimeOffset(new DateTime(2025, 9, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0 },
                    { new Guid("44444444-4444-4444-4444-555555555555"), new Guid("11111111-1111-1111-1111-222222222222"), 60, new Guid("22222222-2222-2222-2222-555555555555"), new Guid("33333333-3333-3333-3333-222222222222"), new DateTimeOffset(new DateTime(2025, 9, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0 },
                    { new Guid("44444444-4444-4444-4444-666666666666"), new Guid("11111111-1111-1111-1111-333333333333"), 30, new Guid("22222222-2222-2222-2222-666666666666"), new Guid("33333333-3333-3333-3333-111111111111"), new DateTimeOffset(new DateTime(2025, 9, 2, 11, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DentistId_StartUtc",
                table: "Appointments",
                columns: new[] { "DentistId", "StartUtc" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_DentistId_StartUtc",
                table: "Appointments");

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-111111111111"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-222222222222"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-333333333333"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-555555555555"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-666666666666"));

            migrationBuilder.DeleteData(
                table: "Dentists",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-222222222222"));

            migrationBuilder.DeleteData(
                table: "Dentists",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-333333333333"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-111111111111"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-333333333333"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-444444444444"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-555555555555"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-666666666666"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-111111111111"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-222222222222"));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Email", "FullName" },
                values: new object[] { "john@example.com", "John Doe" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "DurationMinutes", "Name" },
                values: new object[] { 30, "Cleaning" });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "DurationMinutes", "Name" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), 60, "Filling" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DentistId_StartUtc",
                table: "Appointments",
                columns: new[] { "DentistId", "StartUtc" });
        }
    }
}
