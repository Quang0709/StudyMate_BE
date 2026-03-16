using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCoursePurchaseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_purchases");

            migrationBuilder.DropColumn(
                name: "price",
                table: "courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "courses",
                type: "decimal(10,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "course_purchases",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    payment_status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "completed"),
                    purchase_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    purchase_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    transaction_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_purchases", x => x.id);
                    table.ForeignKey(
                        name: "FK_CoursePurchase_Course",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CoursePurchase_Student",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_purchases_course_id",
                table: "course_purchases",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "UQ__course_purchases_student_course",
                table: "course_purchases",
                columns: new[] { "student_id", "course_id" },
                unique: true);
        }
    }
}
