using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.Services.AuthService;
using PersonalizedLearningPath.Services.ConceptDependencyGraph;
using PersonalizedLearningPath.Services.LearningPathEngine;
using PersonalizedLearningPath.Services.Seeding;
using PersonalizedLearningPath.Services.SkillAssessment;

namespace PersonalizedLearningPath
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            // Add services to the container.

            builder.Services.AddControllers();

            // Services registration
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ISkillAssessmentService, SkillAssessmentService>();
            builder.Services.AddScoped<IConceptDependencyService, ConceptDependencyService>();
            builder.Services.AddScoped<ILearningPathService, LearningPathService>();
            builder.Services.AddScoped<IProgressService, ProgressService>();
            builder.Services.AddScoped<DashboardSeeder>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "https://localhost:62128")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            var app = builder.Build();

            if (args.Any(a => string.Equals(a, "--seed-dashboard", StringComparison.OrdinalIgnoreCase)))
            {
                await using var scope = app.Services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var seeder = scope.ServiceProvider.GetRequiredService<DashboardSeeder>();

                await db.Database.MigrateAsync();
                var result = await seeder.SeedAsync(db);

                Console.WriteLine($"Seeded dashboard data. Instructors={result.Instructors}, Courses={result.Courses}, CourseProfilesCreated={result.CourseProfilesCreated}, CourseProfilesUpdated={result.CourseProfilesUpdated}");
                return;
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAngularApp");

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();
        }
    }
}
