using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCompleteCourseStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chỉ tạo course_sections và section_materials
            // (quiz_questions, quiz_options, front, back đã được tạo ở migration trước)
            
            migrationBuilder.CreateTable(
                name: "course_sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_CourseSection_Course",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "section_materials",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    material_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__section_materials", x => x.id);
                    table.ForeignKey(
                        name: "FK_SectionMaterial_CourseSection",
                        column: x => x.section_id,
                        principalTable: "course_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_sections_course_id",
                table: "course_sections",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_section_materials_section_id",
                table: "section_materials",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "section_materials");

            migrationBuilder.DropTable(
                name: "course_sections");
        }
    }
}
