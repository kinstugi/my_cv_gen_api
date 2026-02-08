namespace my_cv_gen_api.DTOs;

public class ResumeCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<WorkExperienceCreateDto> WorkExperiences { get; set; } = new List<WorkExperienceCreateDto>();
    public List<EducationCreateDto> Educations { get; set; } = new List<EducationCreateDto>();
    public List<LanguageCreateDto> Languages { get; set; } = new List<LanguageCreateDto>();
    public List<ProjectCreateDto> Projects { get; set; } = new List<ProjectCreateDto>();
    public List<string> Skills { get; set; } = new List<string>();
}

public class ResumeUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
}

public class ResumeResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public List<WorkExperienceResponseDto> WorkExperiences { get; set; } = new List<WorkExperienceResponseDto>();
    public List<EducationResponseDto> Educations { get; set; } = new List<EducationResponseDto>();
    public List<LanguageResponseDto> Languages { get; set; } = new List<LanguageResponseDto>();
    public List<ProjectResponseDto> Projects { get; set; } = new List<ProjectResponseDto>();
    public List<string> Skills { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}