using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileStore.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "Phones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 1,
                column: "Condition",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 2,
                column: "Condition",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 3,
                column: "Condition",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 4,
                column: "Condition",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 5,
                column: "Condition",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Phones",
                keyColumn: "Id",
                keyValue: 6,
                column: "Condition",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Phones");
        }
    }
}
