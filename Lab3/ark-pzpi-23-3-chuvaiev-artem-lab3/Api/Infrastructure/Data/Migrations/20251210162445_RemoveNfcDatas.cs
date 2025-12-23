using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNfcDatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NfcDatas_Users_UserId",
                table: "NfcDatas");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NfcDatas",
                table: "NfcDatas");

            migrationBuilder.DropIndex(
                name: "IX_NfcDatas_SerialNumber",
                table: "NfcDatas");

            migrationBuilder.RenameTable(
                name: "NfcDatas",
                newName: "NfcData");

            migrationBuilder.RenameIndex(
                name: "IX_NfcDatas_UserId",
                table: "NfcData",
                newName: "IX_NfcData_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "NfcData",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NfcData",
                table: "NfcData",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AppLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Application = table.Column<string>(type: "text", nullable: false),
                    BoxId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLogs_BoxId",
                table: "AppLogs",
                column: "BoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_NfcData_Users_UserId",
                table: "NfcData",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NfcData_Users_UserId",
                table: "NfcData");

            migrationBuilder.DropTable(
                name: "AppLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NfcData",
                table: "NfcData");

            migrationBuilder.RenameTable(
                name: "NfcData",
                newName: "NfcDatas");

            migrationBuilder.RenameIndex(
                name: "IX_NfcData_UserId",
                table: "NfcDatas",
                newName: "IX_NfcDatas_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "NfcDatas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NfcDatas",
                table: "NfcDatas",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Application = table.Column<string>(type: "text", nullable: false),
                    BoxId = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NfcDatas_SerialNumber",
                table: "NfcDatas",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_BoxId",
                table: "Logs",
                column: "BoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_NfcDatas_Users_UserId",
                table: "NfcDatas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
