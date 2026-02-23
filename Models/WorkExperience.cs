namespace my_cv_gen_api.Models;

public class WorkExperience
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public List<string> Description { get; set; } = new List<string>();
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; } = false;
    public Resume Resume { get; set; } = null!;
}