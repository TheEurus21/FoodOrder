using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations.Saga
{
    /// <inheritdoc />
    public partial class RemoveOrderIdFromSagaState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderSmsSagaState_OrderId",
                table: "OrderSmsSagaState");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "OrderSmsSagaState");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentState",
                table: "OrderSmsSagaState",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CurrentState",
                table: "OrderSmsSagaState",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "OrderSmsSagaState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderSmsSagaState_OrderId",
                table: "OrderSmsSagaState",
                column: "OrderId");
        }
    }
}
