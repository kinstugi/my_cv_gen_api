namespace my_cv_gen_api.Controllers;

public class ResumeController : ControllerBase
{
    private readonly IResumeRepository _resumeRepository;
    public ResumeController(IResumeRepository resumeRepository)
    {
        _resumeRepository = resumeRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateResume([FromBody] ResumeCreateDto dto)
    {
        var resume = await _resumeRepository.CreateResumeAsync(dto, User.Id);
        return Ok(resume);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetResumeById(int id)
    {
        var resume = await _resumeRepository.GetResumeByIdAsync(id);
        if (resume is null)
            return NotFound();
        return Ok(resume);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResume(int id, [FromBody] ResumeUpdateDto dto)
    {
        var resume = await _resumeRepository.UpdateResumeAsync(id, dto, User.Id);
        return Ok(resume);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResume(int id)
    {
        var resume = await _resumeRepository.DeleteResumeAsync(id, User.Id);
        if (resume is null)
            return NotFound();
        return Ok(resume);
    }

    [HttpGet]
    public async Task<IActionResult> GetResumes(int page = 1, int pageSize = 10)
    {
        var resumes = await _resumeRepository.GetResumesByUserIdAsync(User.Id, page, pageSize);
        return Ok(resumes);
    }
}