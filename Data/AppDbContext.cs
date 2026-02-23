using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using my_cv_gen_api.Models;

namespace my_cv_gen_api.Data;

public class SkillsJsonConverter : ValueConverter<List<string>, string>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public SkillsJsonConverter()
        : base(
            v => ToJson(v),
            v => FromJson(v))
    {
    }

    private static string ToJson(List<string> list) => JsonSerializer.Serialize(list, JsonOptions);
    private static List<string> FromJson(string json)
    {
        var list = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
        return list ?? new List<string>();
    }
}

/// <summary>
/// Converts WorkExperience.Description (List&lt;string&gt;) to/from DB.
/// Reads both legacy plain text (newline-separated) and new JSON array format so existing CVs keep working.
/// Writes always as JSON array. Uses explicit JsonSerializerOptions to avoid CS0854 (expression tree + optional args).
/// </summary>
public class WorkExperienceDescriptionConverter : ValueConverter<List<string>, string>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public WorkExperienceDescriptionConverter()
        : base(
            v => ToDb(v),
            v => FromDb(v))
    {
    }

    private static string ToDb(List<string> v) => JsonSerializer.Serialize(v, JsonOptions);

    private static List<string> FromDb(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();
        var trimmed = value.Trim();
        // New format: JSON array e.g. ["bullet1","bullet2"]
        if (trimmed.StartsWith('['))
        {
            var list = JsonSerializer.Deserialize<List<string>>(value, JsonOptions);
            return list ?? new List<string>();
        }
        // Legacy format: plain text, possibly newline-separated
        return trimmed
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Resumes)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resume>()
            .HasMany(r => r.WorkExperiences)
            .WithOne(w => w.Resume)
            .HasForeignKey("ResumeId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resume>()
            .HasMany(r => r.Educations)
            .WithOne(e => e.Resume)
            .HasForeignKey("ResumeId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resume>()
            .HasMany(r => r.Languages)
            .WithOne(l => l.Resume)
            .HasForeignKey("ResumeId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resume>()
            .HasMany(r => r.Projects)
            .WithOne(p => p.Resume)
            .HasForeignKey("ResumeId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resume>()
            .Property(r => r.Skills)
            .HasConversion(new SkillsJsonConverter())
            .HasColumnType("jsonb");

        // WorkExperience.Description: List<string> stored as text (JSON array).
        // Converter accepts both legacy plain text (newline-separated) and JSON for existing rows.
        modelBuilder.Entity<WorkExperience>()
            .Property(w => w.Description)
            .HasConversion(new WorkExperienceDescriptionConverter())
            .HasColumnType("text");
    }
}
