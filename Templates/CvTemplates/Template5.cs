using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

/// <summary>
/// Modern black & white layout inspired by the React template:
/// dark left sidebar (contact, education, skills, languages) and
/// right main column (name, title, summary, experience).
/// </summary>
public class Template5 : ICvTemplate
{
    private const string SidebarBg = "#2d3748";
    private const string AccentLight = "#e2e8f0";
    private const string HeadingColor = "#ffffff";
    private const string BodyColor = "#cbd5e0";

    public void Compose(IDocumentContainer container, CvRenderModel model)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.PageColor("#f7fafc");
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana).FontColor(Colors.Black));

            page.Content().AlignCenter().Column(root =>
            {
                // Card-style page
                root.Item().Background(Colors.White).Padding(0).Row(row =>
                {
                    // --- SIDEBAR ---
                    row.ConstantItem(180)
                        .Background(SidebarBg)
                        .PaddingVertical(30)
                        .PaddingHorizontal(20)
                        .Column(side =>
                        {
                            // Avatar / initials circle
                            side.Item().AlignCenter().Element(c =>
                            {
                                c.Width(80).Height(80)
                                    .Border(3, "#4a5568")
                                    .Background("#4a5568")
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text(string.IsNullOrWhiteSpace(model.Name)
                                        ? " "
                                        : string.Join("", model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s[0])))
                                    .FontSize(20)
                                    .FontColor("#a0aec0")
                                    .SemiBold();
                            });

                            side.Item().PaddingBottom(20);

                            // Contact
                            side.Item().Text("Contact")
                                .FontSize(11)
                                .Bold()
                                .FontColor(HeadingColor);

                            side.Item().PaddingTop(8).PaddingBottom(20).Column(c =>
                            {
                                if (!string.IsNullOrEmpty(model.Phone))
                                    c.Item().Text(model.Phone).FontSize(8).FontColor(BodyColor);
                                if (!string.IsNullOrEmpty(model.Email))
                                    c.Item().Text(model.Email).FontSize(8).FontColor(BodyColor);
                                if (!string.IsNullOrEmpty(model.Location))
                                    c.Item().Text(model.Location).FontSize(8).FontColor(BodyColor);
                                if (string.IsNullOrEmpty(model.Phone) &&
                                    string.IsNullOrEmpty(model.Email) &&
                                    string.IsNullOrEmpty(model.Location))
                                {
                                    c.Item().Text("—").FontSize(8).FontColor(BodyColor);
                                }
                            });

                            // Education
                            if (model.Educations?.Count > 0)
                            {
                                side.Item().Text("Education")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(HeadingColor);

                                side.Item().PaddingTop(8).Column(c =>
                                {
                                    foreach (var edu in model.Educations)
                                    {
                                        var years = $"{edu.StartDate:yyyy} - {(edu.EndDate?.ToString(\"yyyy\") ?? \"Present\")}";
                                        c.Item().Text(years)
                                            .FontSize(8)
                                            .FontColor(BodyColor);
                                        c.Item().Text(edu.Degree)
                                            .FontSize(9)
                                            .Bold()
                                            .FontColor(HeadingColor);
                                        c.Item().Text(edu.School)
                                            .FontSize(8)
                                            .FontColor(BodyColor);
                                        c.Item().PaddingBottom(8);
                                    }
                                });
                            }

                            // Expertise (map from Skills)
                            if (model.Skills?.Count > 0)
                            {
                                side.Item().PaddingTop(12).Text("Expertise")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(HeadingColor);

                                side.Item().PaddingTop(6).Column(c =>
                                {
                                    foreach (var skill in model.Skills)
                                    {
                                        c.Item().Row(r =>
                                        {
                                            r.ConstantItem(6).Text("•").FontSize(7).FontColor("#718096");
                                            r.RelativeItem().Text(skill).FontSize(8).FontColor(BodyColor);
                                        });
                                    }
                                });
                            }

                            // Languages
                            if (model.Languages?.Count > 0)
                            {
                                side.Item().PaddingTop(12).Text("Languages")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(HeadingColor);

                                side.Item().PaddingTop(6).Column(c =>
                                {
                                    foreach (var lang in model.Languages)
                                    {
                                        c.Item().Text($"{lang.Name}")
                                            .FontSize(9)
                                            .FontColor(HeadingColor)
                                            .Bold();
                                    }
                                });
                            }
                        });

                    // --- MAIN COLUMN ---
                    row.RelativeItem()
                        .PaddingVertical(30)
                        .PaddingHorizontal(30)
                        .Column(main =>
                        {
                            // Name & title & summary
                            main.Item().Column(header =>
                            {
                                header.Item().Text(model.Name)
                                    .FontSize(22)
                                    .Bold()
                                    .FontColor("#1a202c");

                                if (!string.IsNullOrEmpty(model.Title))
                                {
                                    header.Item().Text(model.Title)
                                        .FontSize(10)
                                        .FontColor("#718096")
                                        .LetterSpacing(2f)
                                        .Caps();
                                }

                                if (!string.IsNullOrEmpty(model.Summary))
                                {
                                    header.Item().PaddingTop(8)
                                        .Text(model.Summary)
                                        .FontSize(9)
                                        .FontColor("#4a5568")
                                        .LineHeight(1.4f);
                                }
                            });

                            // Experience
                            if (model.WorkExperiences?.Count > 0)
                            {
                                main.Item().PaddingTop(20).Text("Experience")
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor("#1a202c");

                                foreach (var exp in model.WorkExperiences)
                                {
                                    var endStr = exp.IsCurrent || exp.EndDate == null
                                        ? "Present"
                                        : exp.EndDate.Value.ToString("yyyy");

                                    main.Item().PaddingTop(10).Row(expRow =>
                                    {
                                        // Dot column
                                        expRow.ConstantItem(14).AlignTop().Element(c =>
                                        {
                                            c.Width(10).Height(10)
                                                .Border(2, "#a0aec0")
                                                .BorderColor("#a0aec0")
                                                .Background(Colors.White)
                                                .CornerRadius(5);
                                        });

                                        // Details
                                        expRow.RelativeItem().Column(expCol =>
                                        {
                                            expCol.Item().Text($"{exp.StartDate:yyyy} - {endStr}")
                                                .FontSize(8)
                                                .FontColor("#718096");

                                            expCol.Item().Text(exp.Company)
                                                .FontSize(9)
                                                .FontColor("#718096");

                                            expCol.Item().Text(exp.Position)
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor("#1a202c");

                                            if (!string.IsNullOrEmpty(exp.Description))
                                            {
                                                var bullets = exp.Description.Split('\n',
                                                    StringSplitOptions.RemoveEmptyEntries);

                                                expCol.Item().PaddingTop(4).Column(descCol =>
                                                {
                                                    foreach (var bullet in bullets)
                                                    {
                                                        descCol.Item().Text(bullet.Trim())
                                                            .FontSize(9)
                                                            .FontColor("#4a5568")
                                                            .LineHeight(1.4f);
                                                    }
                                                });
                                            }
                                        });
                                    });
                                }
                            }
                        });
                });
            });
        });
    }
}

