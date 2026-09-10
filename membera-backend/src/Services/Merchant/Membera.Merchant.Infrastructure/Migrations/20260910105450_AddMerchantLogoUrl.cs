using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Membera.Merchant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantLogoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Merchants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Merchants");
        }
    }
}
