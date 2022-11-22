using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdersWebApi.Migrations
{
    /// <inheritdoc />
    public partial class OrderaddedOrderDone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OrderDone",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDone",
                table: "Orders");
        }
    }
}
