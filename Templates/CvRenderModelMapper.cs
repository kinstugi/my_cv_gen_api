using my_cv_gen_api.Models;

namespace my_cv_gen_api.Templates;

public static class CvRenderModelMapper
{
    public static CvRenderModel ToRenderModel(Resume resume, User? user = null)
    {
        var name = user != null
            ? $"{user.FirstName} {user.LastName}".Trim()
            : string.Empty;

        return new CvRenderModel
        {
            Name = name,
            Title = resume.Title,
            Summary = resume.Description,
            ImageUrl = resume.ImageUrl,
            Email = user?.Email,
            Phone = user?.PhoneNumber,
            Location = user?.Location,
            GitHubUrl = user?.GitHubUrl,
            Website = user?.Website,
            Skills = resume.Skills.ToList(),
            Languages = resume.Languages.Select(l => new CvLanguage { Name = l.Name, Level = l.Level }).ToList(),
            WorkExperiences = resume.WorkExperiences.Select(w => new CvWorkExperience
            {
                Company = w.Company,
                Position = w.Position,
                Description = w.Description,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                IsCurrent = w.IsCurrent
            }).ToList(),
            Projects = resume.Projects.Select(p => new CvProject
            {
                Title = p.Title,
                Description = p.Description,
                Link = p.Link
            }).ToList(),
            Educations = resume.Educations.Select(e => new CvEducation
            {
                School = e.School,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            }).ToList()
        };
    }
}
