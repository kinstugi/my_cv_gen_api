using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

public class Template3 : ICvTemplate
{
    public void Compose(IDocumentContainer container, CvRenderModel model)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black).FontFamily(Fonts.Verdana));

            page.Content().Column(col =>
            {
                // --- HEADER ---
                col.Item().AlignCenter().Column(header =>
                {
                    header.Item().Text((model.Name ?? "").ToUpper()).FontSize(24).ExtraBold();
                    header.Item().Text(model.Title ?? "").FontSize(13).Medium();
                });

                // --- CONTACT BAR ---
                col.Item().PaddingTop(5).AlignCenter().Row(row =>
                {
                    row.Spacing(10);
                    if (!string.IsNullOrEmpty(model.Phone)) row.AutoItem().Text($"📞 {model.Phone}").FontSize(9);
                    if (!string.IsNullOrEmpty(model.Email)) row.AutoItem().Text($"✉️ {model.Email}").FontSize(9);
                    if (!string.IsNullOrEmpty(model.Location)) row.AutoItem().Text($"📍 {model.Location}").FontSize(9);
                });

                // --- PROFILE ---
                if (!string.IsNullOrEmpty(model.Summary))
                {
                    col.Item().Element(c => ComposeHeader(c, "Profile"));
                    col.Item().PaddingTop(5).Text(model.Summary).Justify().LineHeight(1.2f);
                }

                // --- SKILLS ---
                if (model.Skills?.Count > 0)
                {
                    col.Item().Element(c => ComposeHeader(c, "Skills"));
                    col.Item().PaddingTop(5).Text(string.Join("  •  ", model.Skills)).FontSize(9);
                }

                // --- EXPERIENCE ---
                if (model.WorkExperiences?.Count > 0)
                {
                    col.Item().Element(c => ComposeHeader(c, "Work experience"));

                    foreach (var exp in model.WorkExperiences)
                    {
                        var endStr = exp.IsCurrent || exp.EndDate == null ? "Present" : exp.EndDate.Value.ToString("MMM yyyy");
                        col.Item().PaddingTop(10).Column(expCol =>
                        {
                            expCol.Item().Row(r =>
                            {
                                r.RelativeItem().Text(exp.Position).Bold().FontSize(11);
                                r.ConstantItem(150).AlignRight().Text($"{exp.StartDate:MMM yyyy} - {endStr}").Italic().FontSize(9);
                            });
                            expCol.Item().Text(exp.Company).Italic().FontColor(Colors.Grey.Darken2);

                            if (!string.IsNullOrEmpty(exp.Description))
                            {
                                var bullets = exp.Description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                                foreach (var bullet in bullets)
                                    expCol.Item().PaddingLeft(5).Text($"• {bullet.Trim()}").FontSize(9);
                            }
                        });
                    }
                }

                // --- EDUCATION ---
                if (model.Educations?.Count > 0)
                {
                    col.Item().Element(c => ComposeHeader(c, "Education"));
                    foreach (var edu in model.Educations)
                    {
                        var endStr = edu.EndDate?.ToString("yyyy") ?? "—";
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"{edu.Degree} in {edu.FieldOfStudy}").Bold();
                                c.Item().Text(edu.School).Italic().FontColor(Colors.Grey.Darken2);
                            });
                            row.ConstantItem(100).AlignRight().Text(endStr).Italic().FontSize(9);
                        });
                    }
                }

                // --- PROJECTS ---
                if (model.Projects?.Count > 0)
                {
                    col.Item().Element(c => ComposeHeader(c, "Projects"));

                    foreach (var proj in model.Projects)
                    {
                        col.Item().PaddingTop(10).Column(projCol =>
                        {
                            projCol.Item().Row(row =>
                            {
                                row.RelativeItem().Text(proj.Title).Bold().FontSize(11);
                                if (!string.IsNullOrEmpty(proj.Link))
                                    row.AutoItem().Text(proj.Link).FontSize(9).FontColor(Colors.Blue.Medium).Underline();
                            });

                            if (!string.IsNullOrEmpty(proj.Description))
                                projCol.Item().Text(proj.Description).FontSize(9);
                        });
                    }
                }
            });
        });
    }

    private static void ComposeHeader(IContainer container, string title)
    {
        container
            .PaddingTop(20)
            .PaddingBottom(5)
            .BorderBottom(1)
            .BorderColor(Colors.Black)
            .Text(title)
            .FontSize(14)
            .Bold();
    }
}
