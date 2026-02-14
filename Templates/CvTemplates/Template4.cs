using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

public class Template4 : ICvTemplate
{
    private const string AccentColor = "#b0a48a";

    public void Compose(IDocumentContainer container, CvRenderModel model)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(0);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black).FontFamily(Fonts.Verdana));

            page.Content().Column(col =>
            {
                // --- TAN HEADER BAR ---
                col.Item().Background(AccentColor).PaddingVertical(30).PaddingHorizontal(50).Row(row =>
                {
                    // Profile Image
                    row.ConstantItem(100).Height(100).Width(100)
                        .Background(Colors.White)
                        .AlignCenter()
                        .Element(c =>
                        {
                            if (!string.IsNullOrEmpty(model.ImageUrl))
                            {
                                try
                                {
                                    c.Image(model.ImageUrl);
                                }
                                catch
                                {
                                    // Skip if URL invalid or unreachable
                                }
                            }
                        });

                    row.RelativeItem().PaddingLeft(30).Column(headerCol =>
                    {
                        headerCol.Item().PaddingTop(10);
                        headerCol.Item().Text(model.Name).FontSize(22).ExtraBold().FontColor(Colors.Black);
                        headerCol.Item().Text(model.Title).FontSize(12).SemiBold().FontColor(Colors.Black);

                        // Contact Info Row
                        headerCol.Item().PaddingTop(10).Row(contactRow =>
                        {
                            contactRow.Spacing(15);
                            contactRow.RelativeItem().Column(c =>
                            {
                                if (!string.IsNullOrEmpty(model.Phone)) c.Item().Text($"📞 {model.Phone}").FontSize(8);
                                if (!string.IsNullOrEmpty(model.Email)) c.Item().Text($"✉️ {model.Email}").FontSize(8);
                                if (!string.IsNullOrEmpty(model.Location)) c.Item().Text($"📍 {model.Location}").FontSize(8);
                                if (!string.IsNullOrEmpty(model.GitHubUrl)) c.Item().Text($"🔗 {model.GitHubUrl}").FontSize(8);
                                if (!string.IsNullOrEmpty(model.Website)) c.Item().Text($"🌐 {model.Website}").FontSize(8);
                            });
                        });
                    });
                });

                // --- TWO-COLUMN MAIN BODY ---
                col.Item().PaddingHorizontal(40).PaddingVertical(30).Row(bodyRow =>
                {
                    // LEFT COLUMN (Skills & Languages)
                    bodyRow.ConstantItem(170).Column(sidebar =>
                    {
                        if (model.Skills?.Count > 0)
                        {
                            sidebar.Item().Element(c => ComposeSectionTitle(c, "SKILLS"));
                            foreach (var skill in model.Skills)
                                sidebar.Item().PaddingBottom(4).Text(skill).FontSize(9);
                        }

                        if (model.Languages?.Count > 0)
                        {
                            sidebar.Item().PaddingTop(20).Element(c => ComposeSectionTitle(c, "LANGUAGES"));
                            foreach (var lang in model.Languages)
                                sidebar.Item().PaddingBottom(4).Text($"{lang.Name} ({lang.Level})").FontSize(9);
                        }
                    });

                    bodyRow.ConstantItem(30); // Spacer

                    // RIGHT COLUMN (Profile, Experience, Education, Projects)
                    bodyRow.RelativeItem().Column(main =>
                    {
                        // Profile
                        if (!string.IsNullOrEmpty(model.Summary))
                        {
                            main.Item().Element(c => ComposeSectionTitle(c, "PROFILE"));
                            main.Item().Text(model.Summary).FontSize(9).Justify().LineHeight(1.2f);
                        }

                        // Work Experience
                        if (model.WorkExperiences?.Count > 0)
                        {
                            main.Item().PaddingTop(20).Element(c => ComposeSectionTitle(c, "WORK EXPERIENCE"));
                            foreach (var exp in model.WorkExperiences)
                            {
                                var endStr = exp.IsCurrent || exp.EndDate == null ? "Present" : exp.EndDate.Value.ToString("MMM yyyy");
                                main.Item().PaddingBottom(15).Column(expCol =>
                                {
                                    expCol.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text(exp.Position).Bold().FontSize(10);
                                        r.AutoItem().Text($"{exp.StartDate:MMM yyyy} - {endStr}").FontSize(8).FontColor(Colors.Grey.Medium);
                                    });
                                    expCol.Item().Text(exp.Company).Italic().FontSize(9).FontColor(Colors.Grey.Darken2);

                                    if (!string.IsNullOrEmpty(exp.Description))
                                    {
                                        var bullets = exp.Description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var bullet in bullets)
                                            expCol.Item().PaddingLeft(5).Text($"• {bullet.Trim()}").FontSize(8).LineHeight(1.1f);
                                    }
                                });
                            }
                        }

                        // Education
                        if (model.Educations?.Count > 0)
                        {
                            main.Item().PaddingTop(10).Element(c => ComposeSectionTitle(c, "EDUCATION"));
                            foreach (var edu in model.Educations)
                            {
                                var endStr = edu.EndDate?.ToString("yyyy") ?? "—";
                                main.Item().PaddingBottom(10).Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"{edu.Degree} in {edu.FieldOfStudy}").Bold().FontSize(9);
                                        c.Item().Text($"{edu.School} / {endStr}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                    });
                                });
                            }
                        }

                        // Projects
                        if (model.Projects?.Count > 0)
                        {
                            main.Item().PaddingTop(10).Element(c => ComposeSectionTitle(c, "PROJECTS"));
                            foreach (var proj in model.Projects)
                            {
                                main.Item().PaddingBottom(10).Column(pCol =>
                                {
                                    pCol.Item().Text(proj.Title).Bold().FontSize(9);
                                    if (!string.IsNullOrEmpty(proj.Description))
                                        pCol.Item().Text(proj.Description).FontSize(8).LineHeight(1.1f);
                                });
                            }
                        }
                    });
                });
            });
        });
    }

    private static void ComposeSectionTitle(IContainer container, string title)
    {
        container.PaddingBottom(8).Text(title).FontSize(11).ExtraBold();
    }
}
