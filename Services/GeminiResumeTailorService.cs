using System.Text.Json;
using System.Text.RegularExpressions;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Models;

namespace my_cv_gen_api.Services;

public class GeminiResumeTailorService : IResumeTailorService
{
    private readonly TailorOptions _options;
    private Client? _client;

    public GeminiResumeTailorService(IOptions<TailorOptions> options)
    {
        _options = options.Value;
    }

    private Client GetClient()
    {
        if (_client != null) return _client;
        var apiKey = _options.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Tailor:ApiKey is required. Set Tailor__ApiKey (Google AI API key) in configuration or environment.");
        _client = new Client(apiKey: apiKey);
        return _client;
    }

    public async Task<ResumeCreateDto> TailorResumeAsync(Resume resume, string jobDescription, CancellationToken cancellationToken = default)
    {
        var resumeJson = SerializeResumeForPrompt(resume);
        var systemPrompt = GetSystemPrompt();
        var userPrompt = $"""
            Current resume (JSON):
            {resumeJson}

            Job description to tailor for:
            {jobDescription}

            Return ONLY valid JSON matching the ResumeCreateDto schema. Use ISO date format (yyyy-MM-dd) for dates. 
            Preserve original dates from the resume where relevant. Keep ImageUrl if present.
            """;

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content
            {
                Parts = [new Part { Text = systemPrompt }]
            },
            Temperature = 0.3
        };

        var response = await GetClient().Models.GenerateContentAsync(
            model: _options.Model ?? "gemini-2.0-flash",
            contents: userPrompt,
            config: config
        );

        if (response?.Candidates is not { Count: > 0 } ||
            response.Candidates[0].Content?.Parts is not { Count: > 0 })
            throw new InvalidOperationException("Gemini returned no content. The model may have blocked the response or encountered an error.");

        var responseText = response.Candidates[0].Content.Parts[0].Text ?? "";
        return ParseResponse(responseText, resume);
    }

    private static string GetSystemPrompt()
    {
        return """
            You are a professional CV/resume tailoring expert. Your task is to modify a candidate's resume to better match a job description.

            Return a JSON object with this exact structure (ResumeCreateDto):
            {
              "title": "Professional title tailored to the job",
              "description": "Professional summary tailored to highlight relevant experience",
              "imageUrl": "preserve from input or null",
              "workExperiences": [
                {
                  "company": "string",
                  "position": "string",
                  "description": "tailored description emphasizing relevant achievements",
                  "startDate": "yyyy-MM-dd",
                  "endDate": "yyyy-MM-dd or null",
                  "isCurrent": false
                }
              ],
              "educations": [
                {
                  "school": "string",
                  "degree": "string",
                  "fieldOfStudy": "string",
                  "startDate": "yyyy-MM-dd",
                  "endDate": "yyyy-MM-dd or null"
                }
              ],
              "languages": [{"name": "string", "level": "string"}],
              "projects": [
                {
                  "title": "string",
                  "description": "string",
                  "link": "string or null"
                }
              ],
              "skills": ["string"]
            }

            Rules:
            - Preserve all factual data (companies, dates, schools, etc). Only rephrase descriptions and reorder/emphasize to match the job.
            - Use the exact date format yyyy-MM-dd. For endDate use null if current.
            - Emphasize skills and achievements most relevant to the job description.
            - Keep the same number of work experiences, educations, projects unless the job clearly demands different focus.
            - Output ONLY the JSON object, no markdown or explanation.
            """;
    }

    private static string SerializeResumeForPrompt(Resume r)
    {
        var workExps = r.WorkExperiences.Select(w => new
        {
            w.Company,
            w.Position,
            w.Description,
            StartDate = w.StartDate.ToString("yyyy-MM-dd"),
            EndDate = w.EndDate?.ToString("yyyy-MM-dd"),
            w.IsCurrent
        });
        var educations = r.Educations.Select(e => new
        {
            e.School,
            e.Degree,
            e.FieldOfStudy,
            StartDate = e.StartDate.ToString("yyyy-MM-dd"),
            EndDate = e.EndDate?.ToString("yyyy-MM-dd")
        });
        var projects = r.Projects.Select(p => new { p.Title, p.Description, p.Link });
        var languages = r.Languages.Select(l => new { l.Name, l.Level });

        var obj = new
        {
            r.Title,
            r.Description,
            r.ImageUrl,
            WorkExperiences = workExps,
            Educations = educations,
            Languages = languages,
            Projects = projects,
            Skills = r.Skills
        };
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false });
    }

    private static ResumeCreateDto ParseResponse(string responseText, Resume original)
    {
        var json = ExtractJson(responseText);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var dto = new ResumeCreateDto
        {
            Title = root.GetProperty("title").GetString() ?? original.Title,
            Description = root.GetProperty("description").GetString() ?? original.Description,
            ImageUrl = root.TryGetProperty("imageUrl", out var img) && img.ValueKind == JsonValueKind.String ? img.GetString() : original.ImageUrl,
            WorkExperiences = ParseWorkExperiences(root),
            Educations = ParseEducations(root),
            Languages = ParseLanguages(root),
            Projects = ParseProjects(root),
            Skills = ParseSkills(root)
        };
        return dto;
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        var mdMatch = Regex.Match(trimmed, @"```(?:json)?\s*([\s\S]*?)```");
        if (mdMatch.Success)
            return mdMatch.Groups[1].Value.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
            return trimmed[start..(end + 1)];
        return trimmed;
    }

    private static List<WorkExperienceCreateDto> ParseWorkExperiences(JsonElement root)
    {
        var list = new List<WorkExperienceCreateDto>();
        if (!root.TryGetProperty("workExperiences", out var arr)) return list;
        foreach (var item in arr.EnumerateArray())
        {
            list.Add(new WorkExperienceCreateDto
            {
                Company = item.GetProperty("company").GetString() ?? "",
                Position = item.GetProperty("position").GetString() ?? "",
                Description = item.GetProperty("description").GetString() ?? "",
                StartDate = ParseDate(item, "startDate"),
                EndDate = ParseDateNullable(item, "endDate"),
                IsCurrent = item.TryGetProperty("isCurrent", out var ic) && ic.GetBoolean()
            });
        }
        return list;
    }

    private static List<EducationCreateDto> ParseEducations(JsonElement root)
    {
        var list = new List<EducationCreateDto>();
        if (!root.TryGetProperty("educations", out var arr)) return list;
        foreach (var item in arr.EnumerateArray())
        {
            list.Add(new EducationCreateDto
            {
                School = item.GetProperty("school").GetString() ?? "",
                Degree = item.GetProperty("degree").GetString() ?? "",
                FieldOfStudy = item.GetProperty("fieldOfStudy").GetString() ?? "",
                StartDate = ParseDate(item, "startDate"),
                EndDate = ParseDateNullable(item, "endDate")
            });
        }
        return list;
    }

    private static List<LanguageCreateDto> ParseLanguages(JsonElement root)
    {
        var list = new List<LanguageCreateDto>();
        if (!root.TryGetProperty("languages", out var arr)) return list;
        foreach (var item in arr.EnumerateArray())
        {
            list.Add(new LanguageCreateDto
            {
                Name = item.GetProperty("name").GetString() ?? "",
                Level = item.GetProperty("level").GetString() ?? ""
            });
        }
        return list;
    }

    private static List<ProjectCreateDto> ParseProjects(JsonElement root)
    {
        var list = new List<ProjectCreateDto>();
        if (!root.TryGetProperty("projects", out var arr)) return list;
        foreach (var item in arr.EnumerateArray())
        {
            list.Add(new ProjectCreateDto
            {
                Title = item.GetProperty("title").GetString() ?? "",
                Description = item.GetProperty("description").GetString() ?? "",
                Link = item.TryGetProperty("link", out var link) && link.ValueKind == JsonValueKind.String ? link.GetString() : null
            });
        }
        return list;
    }

    private static List<string> ParseSkills(JsonElement root)
    {
        var list = new List<string>();
        if (!root.TryGetProperty("skills", out var arr)) return list;
        foreach (var item in arr.EnumerateArray())
        {
            var s = item.GetString();
            if (!string.IsNullOrEmpty(s)) list.Add(s);
        }
        return list;
    }

    private static DateTime ParseDate(JsonElement elem, string prop)
    {
        if (!elem.TryGetProperty(prop, out var p)) return DateTime.UtcNow;
        var s = p.GetString();
        return DateTime.TryParse(s, out var d) ? d : DateTime.UtcNow;
    }

    private static DateTime? ParseDateNullable(JsonElement elem, string prop)
    {
        if (!elem.TryGetProperty(prop, out var p)) return null;
        if (p.ValueKind == JsonValueKind.Null) return null;
        var s = p.GetString();
        if (string.IsNullOrEmpty(s)) return null;
        return DateTime.TryParse(s, out var d) ? d : null;
    }
}
