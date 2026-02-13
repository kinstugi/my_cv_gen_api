namespace my_cv_gen_api.Services;

public interface ICvPdfService
{
    byte[] GeneratePdf(Models.Resume resume, Models.User? user, string templateId = "template1");
}
