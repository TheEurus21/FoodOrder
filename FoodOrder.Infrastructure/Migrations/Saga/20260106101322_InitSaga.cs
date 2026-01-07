using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations.Saga
{
    /// <inheritdoc />
    public partial class InitSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderSmsSagaState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    FirstSmsSent = table.Column<bool>(type: "bit", nullable: false),
                    ReadySmsSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSmsSagaState", x => x.CorrelationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderSmsSagaState_OrderId",
                table: "OrderSmsSagaState",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderSmsSagaState");
        }
    }
}
