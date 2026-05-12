using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileStore.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerShopAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShopAddress",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Samsung Galaxy A56");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopAddress",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Samsung Galaxy A55");
        }
    }
}
