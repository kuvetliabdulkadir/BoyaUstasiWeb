using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace boya_usta_web.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentPathsPropertyToContactMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentPaths",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentPaths",
                table: "ContactMessages");
        }
    }
}
