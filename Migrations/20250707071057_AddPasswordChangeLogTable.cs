using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordChangeLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "AttendenceMachines");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AttendenceMachines",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "AttendenceMachines",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceModel",
                table: "AttendenceMachines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AttendenceMachines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AttendenceMachines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendenceMachineConnectionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Machine_IP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Connection_StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Connection_EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordsRead = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendenceMachineConnectionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HR_Swap_Record",
                columns: table => new
                {
                    PK_line_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emp_No = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Emp_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Swap_Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Manual = table.Column<bool>(type: "bit", nullable: false),
                    Shift_In = table.Column<bool>(type: "bit", nullable: true),
                    Shift_Out = table.Column<bool>(type: "bit", nullable: true),
                    Creation_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Machine_IP = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Machine_Port = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_Swap_Record", x => x.PK_line_id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordChangeLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendenceMachineConnectionLogs");

            migrationBuilder.DropTable(
                name: "HR_Swap_Record");

            migrationBuilder.DropTable(
                name: "PasswordChangeLogs");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AttendenceMachines");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AttendenceMachines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "AttendenceMachines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceModel",
                table: "AttendenceMachines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AttendenceMachines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "AttendenceMachines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
