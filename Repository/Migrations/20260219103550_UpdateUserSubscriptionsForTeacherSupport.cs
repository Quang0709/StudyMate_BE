using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSubscriptionsForTeacherSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "student_id",
                table: "user_subscriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "user_subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "teacher_id",
                table: "user_subscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_teacher_id",
                table: "user_subscriptions",
                column: "teacher_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sub_Teacher",
                table: "user_subscriptions",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sub_Teacher",
                table: "user_subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_user_subscriptions_teacher_id",
                table: "user_subscriptions");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "user_subscriptions");

            migrationBuilder.DropColumn(
                name: "teacher_id",
                table: "user_subscriptions");

            migrationBuilder.AlterColumn<int>(
                name: "student_id",
                table: "user_subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
