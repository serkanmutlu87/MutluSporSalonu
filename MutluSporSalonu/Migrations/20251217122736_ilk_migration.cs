using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutluSporSalonu.Migrations
{
    /// <inheritdoc />
    public partial class ilk_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Salonlar",
                columns: table => new
                {
                    SaloNID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalonAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SalonAdres = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SalonAcilisSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    SalonKapanisSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    SalonAciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salonlar", x => x.SaloNID);
                });

            migrationBuilder.CreateTable(
                name: "Uyeler",
                columns: table => new
                {
                    UyeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UyeAdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UyeEposta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UyeSifre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UyeTelefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uyeler", x => x.UyeID);
                });

            migrationBuilder.CreateTable(
                name: "Antrenorler",
                columns: table => new
                {
                    AntrenorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AntrenorAdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AntrenorUzmanlikAlanlari = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AntrenorTelefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AntrenorEposta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AntrenorMusaitlikBaslangic = table.Column<TimeSpan>(type: "time", nullable: false),
                    AntrenorMusaitlikBitis = table.Column<TimeSpan>(type: "time", nullable: false),
                    SporSalonuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Antrenorler", x => x.AntrenorID);
                    table.ForeignKey(
                        name: "FK_Antrenorler_Salonlar_SporSalonuId",
                        column: x => x.SporSalonuId,
                        principalTable: "Salonlar",
                        principalColumn: "SaloNID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hizmetler",
                columns: table => new
                {
                    HizmetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HizmetAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HizmetSureDakika = table.Column<int>(type: "int", nullable: false),
                    HizmetUcret = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HizmetAciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SporSalonuID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hizmetler", x => x.HizmetID);
                    table.ForeignKey(
                        name: "FK_Hizmetler_Salonlar_SporSalonuID",
                        column: x => x.SporSalonuID,
                        principalTable: "Salonlar",
                        principalColumn: "SaloNID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AntrenorHizmet",
                columns: table => new
                {
                    AntrenorlerAntrenorID = table.Column<int>(type: "int", nullable: false),
                    HizmetlerHizmetID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntrenorHizmet", x => new { x.AntrenorlerAntrenorID, x.HizmetlerHizmetID });
                    table.ForeignKey(
                        name: "FK_AntrenorHizmet_Antrenorler_AntrenorlerAntrenorID",
                        column: x => x.AntrenorlerAntrenorID,
                        principalTable: "Antrenorler",
                        principalColumn: "AntrenorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AntrenorHizmet_Hizmetler_HizmetlerHizmetID",
                        column: x => x.HizmetlerHizmetID,
                        principalTable: "Hizmetler",
                        principalColumn: "HizmetID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    RandevuID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UyeID = table.Column<int>(type: "int", nullable: false),
                    AntrenorID = table.Column<int>(type: "int", nullable: false),
                    HizmetID = table.Column<int>(type: "int", nullable: false),
                    SalonID = table.Column<int>(type: "int", nullable: false),
                    SporSalonuSaloNID = table.Column<int>(type: "int", nullable: true),
                    RandevuTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RandevuBaslangicSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    RandevuBitisSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    RandevuUcret = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RandevuOnaylandiMi = table.Column<bool>(type: "bit", nullable: false),
                    RandevuAciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RandevuOlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.RandevuID);
                    table.ForeignKey(
                        name: "FK_Randevular_Antrenorler_AntrenorID",
                        column: x => x.AntrenorID,
                        principalTable: "Antrenorler",
                        principalColumn: "AntrenorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Randevular_Hizmetler_HizmetID",
                        column: x => x.HizmetID,
                        principalTable: "Hizmetler",
                        principalColumn: "HizmetID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Randevular_Salonlar_SporSalonuSaloNID",
                        column: x => x.SporSalonuSaloNID,
                        principalTable: "Salonlar",
                        principalColumn: "SaloNID");
                    table.ForeignKey(
                        name: "FK_Randevular_Uyeler_UyeID",
                        column: x => x.UyeID,
                        principalTable: "Uyeler",
                        principalColumn: "UyeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AntrenorHizmet_HizmetlerHizmetID",
                table: "AntrenorHizmet",
                column: "HizmetlerHizmetID");

            migrationBuilder.CreateIndex(
                name: "IX_Antrenorler_SporSalonuId",
                table: "Antrenorler",
                column: "SporSalonuId");

            migrationBuilder.CreateIndex(
                name: "IX_Hizmetler_SporSalonuID",
                table: "Hizmetler",
                column: "SporSalonuID");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_AntrenorID",
                table: "Randevular",
                column: "AntrenorID");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HizmetID",
                table: "Randevular",
                column: "HizmetID");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_SporSalonuSaloNID",
                table: "Randevular",
                column: "SporSalonuSaloNID");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_UyeID",
                table: "Randevular",
                column: "UyeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AntrenorHizmet");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "Antrenorler");

            migrationBuilder.DropTable(
                name: "Hizmetler");

            migrationBuilder.DropTable(
                name: "Uyeler");

            migrationBuilder.DropTable(
                name: "Salonlar");
        }
    }
}
