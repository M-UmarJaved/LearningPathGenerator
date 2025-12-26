using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalizedLearningPath.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSkillAssessmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make idempotent: DB might already have these renames/column.
            migrationBuilder.Sql("SET @has_old := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'FinalLevel');");
            migrationBuilder.Sql("SET @has_new := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'SkillLevel');");
            migrationBuilder.Sql("SET @sql := IF(@has_old > 0 AND @has_new = 0, 'ALTER TABLE `UserSkillAssessments` RENAME COLUMN `FinalLevel` TO `SkillLevel`;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("SET @has_old := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'AssessedAt');");
            migrationBuilder.Sql("SET @has_new := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'CompletedAt');");
            migrationBuilder.Sql("SET @sql := IF(@has_old > 0 AND @has_new = 0, 'ALTER TABLE `UserSkillAssessments` RENAME COLUMN `AssessedAt` TO `CompletedAt`;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("SET @col_exists := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'TotalAnswered');");
            migrationBuilder.Sql("SET @sql := IF(@col_exists = 0, 'ALTER TABLE `UserSkillAssessments` ADD COLUMN `TotalAnswered` int NOT NULL DEFAULT 0;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET @col_exists := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'TotalAnswered');");
            migrationBuilder.Sql("SET @sql := IF(@col_exists > 0, 'ALTER TABLE `UserSkillAssessments` DROP COLUMN `TotalAnswered`;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("SET @has_old := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'SkillLevel');");
            migrationBuilder.Sql("SET @has_new := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'FinalLevel');");
            migrationBuilder.Sql("SET @sql := IF(@has_old > 0 AND @has_new = 0, 'ALTER TABLE `UserSkillAssessments` RENAME COLUMN `SkillLevel` TO `FinalLevel`;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("SET @has_old := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'CompletedAt');");
            migrationBuilder.Sql("SET @has_new := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserSkillAssessments' AND COLUMN_NAME = 'AssessedAt');");
            migrationBuilder.Sql("SET @sql := IF(@has_old > 0 AND @has_new = 0, 'ALTER TABLE `UserSkillAssessments` RENAME COLUMN `CompletedAt` TO `AssessedAt`;', 'SELECT 1;');");
            migrationBuilder.Sql("PREPARE stmt FROM @sql;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");
        }
    }
}
