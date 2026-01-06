using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace boya_usta_web.Migrations
{
    /// <inheritdoc />
    public partial class AddMinMaxSquareMeters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Statistics",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "MaxSquareMeters",
                table: "ContactMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinSquareMeters",
                table: "ContactMessages",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSquareMeters",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "MinSquareMeters",
                table: "ContactMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Statistics",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
