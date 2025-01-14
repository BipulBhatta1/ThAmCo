using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThAmCo.Customers.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestDeleteToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequestDelete",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestDelete",
                table: "Customers");
        }
    }
}
