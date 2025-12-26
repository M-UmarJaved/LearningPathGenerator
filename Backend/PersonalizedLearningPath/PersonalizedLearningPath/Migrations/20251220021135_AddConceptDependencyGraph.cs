using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalizedLearningPath.Migrations
{
    /// <inheritdoc />
    public partial class AddConceptDependencyGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Concepts",
                columns: table => new
                {
                    ConceptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concepts", x => x.ConceptId);
                    table.ForeignKey(
                        name: "FK_Concepts_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConceptPrerequisites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    ConceptId = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptPrerequisites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptPrerequisites_Concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalTable: "Concepts",
                        principalColumn: "ConceptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConceptPrerequisites_Concepts_PrerequisiteId",
                        column: x => x.PrerequisiteId,
                        principalTable: "Concepts",
                        principalColumn: "ConceptId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptPrerequisites_ConceptId",
                table: "ConceptPrerequisites",
                column: "ConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptPrerequisites_PrerequisiteId",
                table: "ConceptPrerequisites",
                column: "PrerequisiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptPrerequisites_SkillId_ConceptId_PrerequisiteId",
                table: "ConceptPrerequisites",
                columns: new[] { "SkillId", "ConceptId", "PrerequisiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_SkillId_Name",
                table: "Concepts",
                columns: new[] { "SkillId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptPrerequisites");

            migrationBuilder.DropTable(
                name: "Concepts");
        }
    }
}
