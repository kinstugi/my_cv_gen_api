namespace my_cv_gen_api.Templates;

/// <summary>
/// Data model for CV/Resume PDF rendering. Built from Resume + User.
/// </summary>
public class CvRenderModel
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ImageUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Website { get; set; }
    public List<string> Skills { get; set; } = [];
    public List<CvLanguage> Languages { get; set; } = [];
    public List<CvWorkExperience> WorkExperiences { get; set; } = [];
    public List<CvProject> Projects { get; set; } = [];
    public List<CvEducation> Educations { get; set; } = [];
}

public class CvLanguage
{
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}

public class CvWorkExperience
{
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
}

public class CvProject
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Link { get; set; }
}

public class CvEducation
{
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
