using System.Text.Json;
using System.Text.Json.Nodes;
using GroqApiLibrary;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;

namespace my_cv_gen_api.Services;

public class GroqResumeImportService : IGroqResumeImportService
{
    private readonly TailorOptions _options;
    private GroqApiClient? _client;

    public GroqResumeImportService(IOptions<TailorOptions> options)
    {
        _options = options.Value;
    }

    public async Task<JsonElement> ExtractResumeAsync(Stream pdf, CancellationToken cancellationToken = default)
    {
        var apiKey = _options.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Tailor:ApiKey is required. Set Tailor__ApiKey (Groq API key) in configuration or environment.");

        using var document = PdfDocument.Open(pdf);
        var text = string.Join("\n", document.GetPages().Select(page => page.Text));
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("The PDF contains no readable text.");

        var request = new JsonObject
        {
            ["model"] = _options.Model ?? "openai/gpt-oss-120b",
            ["temperature"] = 0.2,
            ["messages"] = new JsonArray
            {
                new JsonObject { ["role"] = "system", ["content"] = Prompt },
                new JsonObject { ["role"] = "user", ["content"] = $"Resume PDF text:\n\n{text}" }
            }
        };

        cancellationToken.ThrowIfCancellationRequested();
        var result = await GetClient().CreateChatCompletionAsync(request);
        var content = result?["choices"]?[0]?["message"]?["content"]?.ToString();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Groq returned no extracted resume data.");

        using var extractedJson = JsonDocument.Parse(ExtractJson(content));
        return extractedJson.RootElement.Clone();
    }

    private GroqApiClient GetClient()
    {
        if (_client != null) return _client;
        _client = new GroqApiClient(_options.ApiKey);
        return _client;
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        return start >= 0 && end > start ? trimmed[start..(end + 1)] : trimmed;
    }

    private const string Prompt = """
        You are a CV/resume parser. Extract the supplied resume text into one valid JSON object with EXACTLY this shape:
        {
          "title": string,
          "description": string,
          "imageUrl": null,
          "workExperiences": [{
            "company": string, "position": string, "description": [string],
            "startDate": string | null, "endDate": string | null, "isCurrent": boolean
          }],
          "educations": [{
            "school": string, "degree": string, "fieldOfStudy": string,
            "startDate": string | null, "endDate": string | null
          }],
          "languages": [{ "name": string, "level": string }],
          "projects": [{ "title": string, "description": string, "link": string | null }],
          "skills": [string]
        }
        Use yyyy-MM-dd dates when possible. Do not invent facts. Use null or empty values when unknown. Respond with JSON only, with no markdown or explanation.
        """;
}
