using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

public class Template2 : ICvTemplate
{
    private const string SidebarColor = "#b0a48a";

    public void Compose(IDocumentContainer container, CvRenderModel model)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(0);
            page.PageColor(Colors.White);

            page.Content().Row(row =>
            {
                // --- MAIN CONTENT (Left) ---
                row.RelativeItem()
                    .PaddingVertical(50)
                    .PaddingHorizontal(40)
                    .Column(column =>
                    {
                        // Header
                        column.Item().Text(model.Name).FontSize(32).Bold().FontColor(Colors.Black);
                        if (!string.IsNullOrEmpty(model.Title))
                            column.Item().Text(model.Title).FontSize(14).FontColor(Colors.Grey.Darken2);
                        column.Item().PaddingBottom(30);

                        // Summary
                        if (!string.IsNullOrEmpty(model.Summary))
                        {
                            column.Item().Text("PROFESSIONAL SUMMARY").FontSize(14).Bold().FontColor(Colors.Black);
                            column.Item().PaddingBottom(10);
                            column.Item().Text(model.Summary).FontSize(11).FontColor(Colors.Black).LineHeight(1.6f);
                            column.Item().PaddingBottom(30);
                        }

                        // Experience
                        if (model.WorkExperiences?.Count > 0)
                        {
                            column.Item().Text("EXPERIENCE").FontSize(14).Bold().FontColor(Colors.Black);
                            column.Item().PaddingBottom(15);

                            foreach (var exp in model.WorkExperiences)
                            {
                                var endStr = exp.IsCurrent || exp.EndDate == null ? "Present" : exp.EndDate.Value.ToString("MMM yyyy");
                                column.Item().Row(r =>
                                {
                                    r.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text(exp.Position).FontSize(13).Bold();
                                        col.Item().Text(exp.Company).FontSize(11).FontColor(SidebarColor);
                                    });
                                    r.AutoItem()
                                        .Text($"{exp.StartDate:MMM yyyy} - {endStr}")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Darken2);
                                });

                                if (!string.IsNullOrEmpty(exp.Description))
                                {
                                    column.Item().PaddingTop(8);
                                    var bullets = exp.Description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var desc in bullets)
                                        column.Item().Text($"• {desc.Trim()}").FontSize(10).LineHeight(1.5f);
                                }

                                column.Item().PaddingBottom(20);
                            }
                            column.Item().PaddingBottom(30);
                        }

                        // Education
                        if (model.Educations?.Count > 0)
                        {
                            column.Item().Text("EDUCATION").FontSize(14).Bold().FontColor(Colors.Black);
                            column.Item().PaddingBottom(15);

                            foreach (var edu in model.Educations)
                            {
                                var startStr = edu.StartDate.ToString("yyyy");
                                var endStr = edu.EndDate?.ToString("yyyy") ?? "—";
                                column.Item().Row(r =>
                                {
                                    r.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text($"{edu.Degree} in {edu.FieldOfStudy}").FontSize(13).Bold();
                                        col.Item().Text(edu.School).FontSize(11).FontColor(SidebarColor);
                                    });
                                    r.AutoItem().Text($"{startStr} - {endStr}").FontSize(10).FontColor(Colors.Grey.Darken2);
                                });
                                column.Item().PaddingBottom(20);
                            }
                            column.Item().PaddingBottom(30);
                        }

                        // Projects
                        if (model.Projects?.Count > 0)
                        {
                            column.Item().Text("PROJECTS").FontSize(14).Bold().FontColor(Colors.Black);
                            column.Item().PaddingBottom(15);

                            foreach (var proj in model.Projects)
                            {
                                column.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(proj.Title).FontSize(13).Bold();
                                    if (!string.IsNullOrEmpty(proj.Link))
                                        r.AutoItem().Text(proj.Link).FontSize(10).FontColor(Colors.Blue.Darken2).Underline();
                                });

                                if (!string.IsNullOrEmpty(proj.Description))
                                    column.Item().Text(proj.Description).FontSize(10).LineHeight(1.5f);

                                column.Item().PaddingBottom(20);
                            }
                        }
                    });

                // --- SIDEBAR (Right) ---
                row.ConstantItem(190)
                    .Background(SidebarColor)
                    .PaddingVertical(50)
                    .PaddingHorizontal(20)
                    .Column(column =>
                    {
                        // Profile Image
                        if (!string.IsNullOrEmpty(model.ImageUrl))
                        {
                            try
                            {
                                column.Item()
                                    .Width(130)
                                    .Height(130)
                                    .AlignCenter()
                                    .Image(model.ImageUrl);
                            }
                            catch
                            {
                                // Skip image if URL invalid
                            }
                            column.Item().PaddingBottom(40);
                        }

                        // Contact
                        column.Item().Text("CONTACT").FontSize(12).Bold().FontColor(Colors.White);
                        column.Item().PaddingBottom(15);

                        if (!string.IsNullOrEmpty(model.Location))
                            column.Item().PaddingBottom(5).Text(model.Location).FontSize(9).FontColor(Colors.White);
                        if (!string.IsNullOrEmpty(model.Phone))
                            column.Item().PaddingBottom(5).Text(model.Phone).FontSize(9).FontColor(Colors.White);
                        if (!string.IsNullOrEmpty(model.Email))
                            column.Item().PaddingBottom(5).Text(model.Email).FontSize(9).FontColor(Colors.White);
                        if (string.IsNullOrEmpty(model.Location) && string.IsNullOrEmpty(model.Phone) && string.IsNullOrEmpty(model.Email))
                            column.Item().Text("—").FontSize(9).FontColor(Colors.White);

                        column.Item().PaddingBottom(30);

                        // Skills
                        if (model.Skills?.Count > 0)
                        {
                            column.Item().Text("SKILLS").FontSize(12).Bold().FontColor(Colors.White);
                            column.Item().PaddingBottom(10);
                            foreach (var skill in model.Skills)
                                column.Item().PaddingBottom(3).Text($"• {skill}").FontSize(9).FontColor(Colors.White);
                            column.Item().PaddingBottom(30);
                        }

                        // Languages
                        if (model.Languages?.Count > 0)
                        {
                            column.Item().Text("LANGUAGES").FontSize(12).Bold().FontColor(Colors.White);
                            column.Item().PaddingBottom(10);
                            foreach (var lang in model.Languages)
                                column.Item().PaddingBottom(3).Text($"• {lang.Name} ({lang.Level})").FontSize(9).FontColor(Colors.White);
                        }
                    });
            });
        });
    }
}
