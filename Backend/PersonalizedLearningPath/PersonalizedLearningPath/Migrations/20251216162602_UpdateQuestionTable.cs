using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalizedLearningPath.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make idempotent: if DB already has TreeIndex, don't fail.
            migrationBuilder.Sql("SET @col_exists := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Questions' AND COLUMN_NAME = 'TreeIndex');");
            migrationBuilder.Sql("SET @sql := IF(@col_exists = 0, 'ALTER TABLE `Questions` ADD COLUMN `TreeIndex` int NOT NULL DEFAULT 0;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET @col_exists := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Questions' AND COLUMN_NAME = 'TreeIndex');");
            migrationBuilder.Sql("SET @sql := IF(@col_exists > 0, 'ALTER TABLE `Questions` DROP COLUMN `TreeIndex`;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");
        }
    }
}
