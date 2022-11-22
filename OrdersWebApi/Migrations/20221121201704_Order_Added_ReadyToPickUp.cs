using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdersWebApi.Migrations
{
    /// <inheritdoc />
    public partial class OrderAddedReadyToPickUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReadyToPickUp",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadyToPickUp",
                table: "Orders");
        }
    }
}
