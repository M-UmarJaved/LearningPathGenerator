using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Tables
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<UserSkillAssessment> UserSkillAssessments { get; set; } = null!;

      // Intelligence Core: Courses + Learning Paths + Progress
      public DbSet<Course> Courses { get; set; } = null!;
      public DbSet<CourseVideo> CourseVideos { get; set; } = null!;
      public DbSet<LearningPath> LearningPaths { get; set; } = null!;
      public DbSet<LearningPathCourse> LearningPathCourses { get; set; } = null!;
      public DbSet<UserVideoProgress> UserVideoProgress { get; set; } = null!;
      public DbSet<UserSkillHistory> UserSkillHistory { get; set; } = null!;

      // Dashboard: metadata + activity + notifications
      public DbSet<Instructor> Instructors { get; set; } = null!;
      public DbSet<CourseProfile> CourseProfiles { get; set; } = null!;
      public DbSet<UserCourseEnrollment> UserCourseEnrollments { get; set; } = null!;
      public DbSet<UserNotification> UserNotifications { get; set; } = null!;
      public DbSet<UserActivity> UserActivities { get; set; } = null!;

      // Module 3: Concept Dependency Graph
      public DbSet<Concept> Concepts { get; set; } = null!;
      public DbSet<ConceptPrerequisite> ConceptPrerequisites { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User table configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.Email)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.PasswordHash)
                  .IsRequired();

            entity.Property(u => u.PasswordResetTokenHash)
                  .HasMaxLength(200);

            entity.Property(u => u.PasswordResetTokenExpiresAt);
        });

        // Question table configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(q => q.QuestionId);

            entity.Property(q => q.QuestionText).IsRequired();
            entity.Property(q => q.ChoiceA).IsRequired();
            entity.Property(q => q.ChoiceB).IsRequired();
            entity.Property(q => q.ChoiceC).IsRequired();
            entity.Property(q => q.CorrectAnswer).IsRequired();
            entity.Property(q => q.DifficultyLevel).IsRequired();

            // Relationship: Question -> Skill (many-to-one)
            entity.HasOne(q => q.Skill)
                  .WithMany(s => s.Questions)
                  .HasForeignKey(q => q.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Skill table configuration
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skills");
            entity.HasKey(s => s.SkillId);

            entity.Property(s => s.SkillName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(s => s.Description)
                  .HasMaxLength(500);

            entity.Property(s => s.IsActive)
                  .IsRequired()
                  .HasDefaultValue(true);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(c => c.CourseId);

            entity.Property(c => c.CourseTitle)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(c => c.CourseLevel)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(c => c.YoutubeVideoUrl)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(c => c.TotalVideos)
                  .IsRequired();

            entity.Property(c => c.SequenceOrder)
                  .IsRequired();

            entity.HasOne(c => c.Skill)
                  .WithMany(s => s.Courses)
                  .HasForeignKey(c => c.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.SkillId, c.CourseLevel, c.SequenceOrder });
        });

        modelBuilder.Entity<CourseVideo>(entity =>
        {
            entity.ToTable("CourseVideos");
            entity.HasKey(v => v.CourseVideoId);

            entity.Property(v => v.VideoIndex)
                  .IsRequired();

            entity.Property(v => v.VideoTitle)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(v => v.YoutubeVideoUrl)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.HasOne(v => v.Course)
                  .WithMany(c => c.Videos)
                  .HasForeignKey(v => v.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(v => new { v.CourseId, v.VideoIndex }).IsUnique();
        });

        modelBuilder.Entity<LearningPath>(entity =>
        {
            entity.ToTable("LearningPaths");
            entity.HasKey(lp => lp.PathId);

            entity.Property(lp => lp.Status)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("Active");

            // MySQL-friendly CreatedAt default
            entity.Property(lp => lp.CreatedAt)
                  .IsRequired()
                  .HasColumnType("datetime(6)")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                  .ValueGeneratedOnAdd();

            entity.HasOne(lp => lp.User)
                  .WithMany(u => u.LearningPaths)
                  .HasForeignKey(lp => lp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lp => lp.Skill)
                  .WithMany(s => s.LearningPaths)
                  .HasForeignKey(lp => lp.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(lp => new { lp.UserId, lp.SkillId, lp.Status });
        });

        modelBuilder.Entity<LearningPathCourse>(entity =>
        {
            entity.ToTable("LearningPathCourses");
            entity.HasKey(lpc => lpc.PathCourseId);

            entity.Property(lpc => lpc.CompletionPercentage)
                  .IsRequired()
                  .HasDefaultValue(0);

            entity.Property(lpc => lpc.IsCompleted)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.HasOne(lpc => lpc.LearningPath)
                  .WithMany(lp => lp.LearningPathCourses)
                  .HasForeignKey(lpc => lpc.PathId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lpc => lpc.Course)
                  .WithMany(c => c.LearningPathCourses)
                  .HasForeignKey(lpc => lpc.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(lpc => new { lpc.PathId, lpc.CourseId }).IsUnique();
        });

        modelBuilder.Entity<UserVideoProgress>(entity =>
        {
            entity.ToTable("UserVideoProgress");
            entity.HasKey(p => p.ProgressId);

            entity.Property(p => p.VideoIndex)
                  .IsRequired();

            entity.Property(p => p.IsWatched)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.HasOne(p => p.User)
                  .WithMany(u => u.UserVideoProgress)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Course)
                  .WithMany(c => c.UserVideoProgress)
                  .HasForeignKey(p => p.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => new { p.UserId, p.CourseId, p.VideoIndex }).IsUnique();
        });

        modelBuilder.Entity<UserSkillHistory>(entity =>
        {
            entity.ToTable("UserSkillsHistory");
            entity.HasKey(h => h.HistoryId);

            entity.Property(h => h.CompletedAt)
                  .IsRequired()
                  .HasColumnType("datetime(6)")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                  .ValueGeneratedOnAdd();

            entity.HasOne(h => h.User)
                  .WithMany(u => u.UserSkillHistory)
                  .HasForeignKey(h => h.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.Skill)
                  .WithMany()
                  .HasForeignKey(h => h.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => new { h.UserId, h.SkillId, h.CompletedAt });
        });

        // UserSkillAssessment table configuration
        modelBuilder.Entity<UserSkillAssessment>(entity =>
        {
            entity.ToTable("UserSkillAssessments");
            entity.HasKey(usa => usa.Id);

            entity.Property(usa => usa.SkillLevel)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(usa => usa.CorrectAnswers)
                  .IsRequired();

            // Set DB-provider-appropriate default for CompletedAt to avoid invalid-default errors
            var provider = Database.ProviderName ?? string.Empty;
            if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                provider.Contains("MariaDb", StringComparison.OrdinalIgnoreCase))
            {
                // MySQL / MariaDB: use datetime(6) with precision and CURRENT_TIMESTAMP(6)
                entity.Property(usa => usa.CompletedAt)
                      .IsRequired()
                      .HasColumnType("datetime(6)")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                      .ValueGeneratedOnAdd();
            }
            else if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) ||
                     provider.Contains("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                // SQL Server: use datetime2 and GETUTCDATE()
                entity.Property(usa => usa.CompletedAt)
                      .IsRequired()
                      .HasColumnType("datetime2")
                      .HasDefaultValueSql("GETUTCDATE()")
                      .ValueGeneratedOnAdd();
            }
            else
            {
                // Generic fallback
                entity.Property(usa => usa.CompletedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
            }

            // Relationships
            entity.HasOne(usa => usa.User)
                  .WithMany(u => u.UserSkillAssessments)
                  .HasForeignKey(usa => usa.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(usa => usa.Skill)
                  .WithMany(s => s.UserSkillAssessments)
                  .HasForeignKey(usa => usa.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Instructor
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.ToTable("Instructors");
            entity.HasKey(i => i.InstructorId);

            entity.Property(i => i.FullName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(i => i.Title)
                  .HasMaxLength(100);

            entity.HasIndex(i => i.FullName);
        });

        // CourseProfile (1:1 with Course)
        modelBuilder.Entity<CourseProfile>(entity =>
        {
            entity.ToTable("CourseProfiles");
            entity.HasKey(cp => cp.CourseId);

            entity.Property(cp => cp.ThumbnailUrl)
                  .HasMaxLength(500);

            entity.Property(cp => cp.Category)
                  .HasMaxLength(100);

            entity.Property(cp => cp.Rating)
                  .HasPrecision(3, 2)
                  .HasDefaultValue(0m);

            entity.Property(cp => cp.EnrolledCount)
                  .HasDefaultValue(0);

            entity.HasOne(cp => cp.Course)
                  .WithOne()
                  .HasForeignKey<CourseProfile>(cp => cp.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cp => cp.Instructor)
                  .WithMany(i => i.CourseProfiles)
                  .HasForeignKey(cp => cp.InstructorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // UserCourseEnrollment
        modelBuilder.Entity<UserCourseEnrollment>(entity =>
        {
            entity.ToTable("UserCourseEnrollments");
            entity.HasKey(e => e.EnrollmentId);

            var provider = Database.ProviderName ?? string.Empty;
            if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                provider.Contains("MariaDb", StringComparison.OrdinalIgnoreCase))
            {
                entity.Property(e => e.EnrolledAt)
                      .IsRequired()
                      .HasColumnType("datetime(6)")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                      .ValueGeneratedOnAdd();
            }
            else if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) ||
                     provider.Contains("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                entity.Property(e => e.EnrolledAt)
                      .IsRequired()
                      .HasColumnType("datetime2")
                      .HasDefaultValueSql("GETUTCDATE()")
                      .ValueGeneratedOnAdd();
            }
            else
            {
                entity.Property(e => e.EnrolledAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
            }

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                  .WithMany()
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        });

        // UserNotification
        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications");
            entity.HasKey(n => n.NotificationId);

            entity.Property(n => n.Type)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(n => n.Message)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(n => n.IsRead)
                  .IsRequired()
                  .HasDefaultValue(false);

            var provider = Database.ProviderName ?? string.Empty;
            if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                provider.Contains("MariaDb", StringComparison.OrdinalIgnoreCase))
            {
                entity.Property(n => n.CreatedAt)
                      .IsRequired()
                      .HasColumnType("datetime(6)")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                      .ValueGeneratedOnAdd();
            }
            else if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) ||
                     provider.Contains("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                entity.Property(n => n.CreatedAt)
                      .IsRequired()
                      .HasColumnType("datetime2")
                      .HasDefaultValueSql("GETUTCDATE()")
                      .ValueGeneratedOnAdd();
            }
            else
            {
                entity.Property(n => n.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
            }

            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
        });

        // UserActivity
        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.ToTable("UserActivities");
            entity.HasKey(a => a.ActivityId);

            entity.Property(a => a.Action)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(a => a.Label)
                  .IsRequired()
                  .HasMaxLength(200);

            var provider = Database.ProviderName ?? string.Empty;
            if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                provider.Contains("MariaDb", StringComparison.OrdinalIgnoreCase))
            {
                entity.Property(a => a.CreatedAt)
                      .IsRequired()
                      .HasColumnType("datetime(6)")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                      .ValueGeneratedOnAdd();
            }
            else if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) ||
                     provider.Contains("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                entity.Property(a => a.CreatedAt)
                      .IsRequired()
                      .HasColumnType("datetime2")
                      .HasDefaultValueSql("GETUTCDATE()")
                      .ValueGeneratedOnAdd();
            }
            else
            {
                entity.Property(a => a.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
            }

            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Course)
                  .WithMany()
                  .HasForeignKey(a => a.CourseId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.Skill)
                  .WithMany()
                  .HasForeignKey(a => a.SkillId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.LearningPath)
                  .WithMany()
                  .HasForeignKey(a => a.PathId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(a => new { a.UserId, a.CreatedAt });
        });

        // Concept table configuration
        modelBuilder.Entity<Concept>(entity =>
        {
            entity.ToTable("Concepts");
            entity.HasKey(c => c.ConceptId);

            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(c => c.Description)
                  .HasMaxLength(500);

            entity.HasOne(c => c.Skill)
                  .WithMany()
                  .HasForeignKey(c => c.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique concept names per skill
            entity.HasIndex(c => new { c.SkillId, c.Name }).IsUnique();
        });

        // ConceptPrerequisite table configuration
        modelBuilder.Entity<ConceptPrerequisite>(entity =>
        {
            entity.ToTable("ConceptPrerequisites");
            entity.HasKey(cp => cp.Id);

            entity.HasIndex(cp => new { cp.SkillId, cp.ConceptId, cp.PrerequisiteId }).IsUnique();

            entity.HasOne(cp => cp.Concept)
                  .WithMany(c => c.Prerequisites)
                  .HasForeignKey(cp => cp.ConceptId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cp => cp.Prerequisite)
                  .WithMany(c => c.Dependents)
                  .HasForeignKey(cp => cp.PrerequisiteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
