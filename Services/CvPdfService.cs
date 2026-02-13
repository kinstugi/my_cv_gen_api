using my_cv_gen_api.Templates;
using my_cv_gen_api.Templates.CvTemplates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Services;

public class CvPdfService : ICvPdfService
{
    public CvPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(Models.Resume resume, Models.User? user, string templateId = "template1")
    {
        var model = CvRenderModelMapper.ToRenderModel(resume, user);
        var template = GetTemplate(templateId);
        return Document.Create(container => template.Compose(container, model)).GeneratePdf();
    }

    private static ICvTemplate GetTemplate(string templateId)
    {
        return templateId.ToLowerInvariant() switch
        {
            "template1" or "1" => new Template1(),
            "template2" or "2" => new Template2(),
            "template3" or "3" => new Template3(),
            "template4" or "4" => new Template4(),
            _ => new Template1()
        };
    }
}
