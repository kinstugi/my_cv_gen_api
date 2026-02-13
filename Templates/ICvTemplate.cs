namespace my_cv_gen_api.Templates;

public interface ICvTemplate
{
    void Compose(QuestPDF.Infrastructure.IDocumentContainer container, CvRenderModel model);
}
