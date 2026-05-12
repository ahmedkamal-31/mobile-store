using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileStore.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneSellerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerId",
                table: "Phones",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 1,
                column: "SellerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 2,
                column: "SellerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 3,
                column: "SellerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 4,
                column: "SellerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 5,
                column: "SellerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 6,
                column: "SellerId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Phones_SellerId",
                table: "Phones",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Phones_AspNetUsers_SellerId",
                table: "Phones",
                column: "SellerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phones_AspNetUsers_SellerId",
                table: "Phones");

            migrationBuilder.DropIndex(
                name: "IX_Phones_SellerId",
                table: "Phones");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Phones");
        }
    }
}
