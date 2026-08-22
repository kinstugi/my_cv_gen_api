using System.Text.Json;

namespace my_cv_gen_api.Services;

public interface IGroqResumeImportService
{
    Task<JsonElement> ExtractResumeAsync(Stream pdf, CancellationToken cancellationToken = default);
}
