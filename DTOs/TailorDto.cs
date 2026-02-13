namespace my_cv_gen_api.DTOs;

public class TailorRequestDto
{
    public string JobDescription { get; set; } = string.Empty;
    public bool CreateNewCV { get; set; } = false;
}
