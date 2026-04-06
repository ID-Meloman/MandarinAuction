using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MandarinAuction.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMandarins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Mandarins",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Mandarins");
        }
    }
}
