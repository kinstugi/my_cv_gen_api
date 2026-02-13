using my_cv_gen_api.DTOs;
using my_cv_gen_api.Models;

namespace my_cv_gen_api.Services;

public interface IResumeTailorService
{
    Task<ResumeCreateDto> TailorResumeAsync(Resume resume, string jobDescription, CancellationToken cancellationToken = default);
}
