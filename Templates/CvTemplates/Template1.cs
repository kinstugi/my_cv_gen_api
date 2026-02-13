using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

public class Template1 : ICvTemplate
{
    private const string SidebarColor = "#e9e5d9";
    private const string AccentGold = "#b29b7d";
    private const string TextDark = "#333333";

    public void Compose(IDocumentContainer container, CvRenderModel model)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(0);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Verdana).FontColor(TextDark));

            page.Content().Row(row =>
            {
                // --- SIDEBAR (180pt) ---
                row.ConstantItem(180)
                    .Background(SidebarColor)
                    .PaddingVertical(40)
                    .PaddingHorizontal(20)
                    .Column(column =>
                    {
                        if (!string.IsNullOrEmpty(model.ImageUrl))
                        {
                            try
                            {
                                column.Item().AlignCenter().Width(110).Height(110).Image(model.ImageUrl);
                            }
                            catch
                            {
                                // Skip image if URL invalid or unreachable
                            }
                        }

                        column.Item().PaddingBottom(30);

                        // Contact
                        column.Item().Text("CONTACT").FontSize(11).Bold();
                        column.Item().PaddingTop(8).PaddingBottom(25).Column(c =>
                        {
                            if (!string.IsNullOrEmpty(model.Phone))
                                c.Item().PaddingBottom(5).Text($"📞 {model.Phone}").FontSize(8);
                            if (!string.IsNullOrEmpty(model.Email))
                                c.Item().PaddingBottom(5).Text($"✉️ {model.Email}").FontSize(8);
                            if (!string.IsNullOrEmpty(model.Location))
                                c.Item().PaddingBottom(5).Text($"📍 {model.Location}").FontSize(8);
                            if (string.IsNullOrEmpty(model.Phone) && string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.Location))
                                c.Item().Text("—").FontSize(8);
                        });

                        // Skills
                        if (model.Skills?.Count > 0)
                        {
                            column.Item().Text("SKILLS").FontSize(11).Bold();
                            column.Item().PaddingTop(8).Column(s =>
                            {
                                foreach (var skill in model.Skills)
                                    s.Item().PaddingBottom(3).Text($"• {skill}").FontSize(8);
                            });
                        }

                        // Languages
                        if (model.Languages?.Count > 0)
                        {
                            column.Item().PaddingTop(25).Text("LANGUAGES").FontSize(11).Bold();
                            column.Item().PaddingTop(8).Column(l =>
                            {
                                foreach (var lang in model.Languages)
                                    l.Item().PaddingBottom(3).Text($"• {lang.Name} ({lang.Level})").FontSize(8);
                            });
                        }
                    });

                // --- MAIN CONTENT ---
                row.RelativeItem()
                    .PaddingVertical(50)
                    .PaddingHorizontal(35)
                    .Column(main =>
                    {
                        // Header
                        main.Item().Text(model.Name).FontSize(24).Bold().FontColor(Colors.Black);
                        main.Item().Text(model.Title).FontSize(14).FontColor(AccentGold).SemiBold();
                        main.Item().PaddingBottom(25);

                        // Profile
                        if (!string.IsNullOrEmpty(model.Summary))
                        {
                            main.Item().Text("PROFILE").FontSize(12).Bold();
                            main.Item().PaddingTop(5).PaddingBottom(30).Text(model.Summary).FontSize(9).LineHeight(1.3f).Justify();
                        }

                        // Work Experience
                        if (model.WorkExperiences?.Count > 0)
                        {
                            main.Item().Text("WORK EXPERIENCE").FontSize(12).Bold();
                            main.Item().PaddingTop(15);

                            foreach (var exp in model.WorkExperiences)
                            {
                                main.Item().PaddingBottom(20).Column(expCol =>
                                {
                                    var endDate = exp.IsCurrent || exp.EndDate == null
                                        ? "Present"
                                        : exp.EndDate.Value.ToString("MMM yyyy");
                                    expCol.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text(exp.Position).Bold().FontSize(10);
                                        r.AutoItem()
                                            .Text($"{exp.StartDate:MMM yyyy} - {endDate}")
                                            .FontSize(8)
                                            .FontColor(Colors.Grey.Medium);
                                    });
                                    expCol.Item().Text(exp.Company).FontSize(9).FontColor(AccentGold).SemiBold();

                                    if (!string.IsNullOrEmpty(exp.Description))
                                    {
                                        expCol.Item().PaddingTop(5);
                                        var bullets = exp.Description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var bullet in bullets)
                                            expCol.Item().PaddingLeft(8).Text($"• {bullet.Trim()}").FontSize(8.5f).LineHeight(1.2f);
                                    }
                                });
                            }
                        }

                        // Projects
                        if (model.Projects?.Count > 0)
                        {
                            main.Item().PaddingTop(10).Text("PROJECTS").FontSize(12).Bold();
                            main.Item().PaddingTop(15);

                            foreach (var proj in model.Projects)
                            {
                                main.Item().PaddingBottom(15).Column(projCol =>
                                {
                                    projCol.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text(proj.Title).Bold().FontSize(10);
                                        if (!string.IsNullOrEmpty(proj.Link))
                                            r.AutoItem().Text(proj.Link).FontSize(8).FontColor(Colors.Blue.Medium).Underline();
                                    });

                                    if (!string.IsNullOrEmpty(proj.Description))
                                        projCol.Item().PaddingTop(2).Text(proj.Description).FontSize(8.5f).LineHeight(1.2f);
                                });
                            }
                        }

                        // Education
                        if (model.Educations?.Count > 0)
                        {
                            main.Item().PaddingTop(10).Text("EDUCATION").FontSize(12).Bold();
                            main.Item().PaddingTop(15);

                            foreach (var edu in model.Educations)
                            {
                                main.Item().PaddingBottom(15).Column(eduCol =>
                                {
                                    var endStr = edu.EndDate?.ToString("yyyy") ?? "—";
                                    eduCol.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text($"{edu.Degree} in {edu.FieldOfStudy}").Bold().FontSize(10);
                                        r.AutoItem().Text(endStr).FontSize(8).FontColor(Colors.Grey.Medium);
                                    });
                                    eduCol.Item().Text(edu.School).FontSize(9).FontColor(AccentGold);
                                });
                            }
                        }
                    });
            });
        });
    }
}
