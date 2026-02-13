using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using my_cv_gen_api.Models;

namespace my_cv_gen_api.Data;

public class SkillsJsonConverter : ValueConverter<List<string>, string>
{
    public SkillsJsonConverter()
        : base(v => ToJson(v), v => FromJson(v))
    {
    }

    private static string ToJson(List<string> list) => JsonSerializer.Serialize(list);
    private static List<string> FromJson(string json)
    {
        var list = JsonSerializer.Deserialize<List<string>>(json);
        return list ?? new List<string>();
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
    }
}
