namespace my_cv_gen_api.DTOs;

public class LanguageCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}

public class LanguageResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}