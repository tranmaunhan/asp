using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busline_project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeatTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RowIndex",
                table: "SeatTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ColIndex",
                table: "SeatTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Deck",
                table: "SeatTemplates",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatType",
                table: "SeatTemplates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deck",
                table: "SeatTemplates");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "SeatTemplates");

            migrationBuilder.AlterColumn<int>(
                name: "RowIndex",
                table: "SeatTemplates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ColIndex",
                table: "SeatTemplates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
