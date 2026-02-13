using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace my_cv_gen_api.Models;

public class Resume
{
    public int Id { get; set; }
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkExperience> WorkExperiences { get; } = new List<WorkExperience>();
    public List<Education> Educations { get; } = new List<Education>();
    public List<Language> Languages { get; } = new List<Language>();
    public List<Project> Projects { get; } = new List<Project>();
    public User User { get; set; } = null!;
    public List<string> Skills { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; } = null;
}