namespace my_cv_gen_api.Models;

public class Language
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public Resume Resume { get; set; } = null!;
}