using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Membera.Merchant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantBusinessCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Merchants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Other");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Merchants");
        }
    }
}
