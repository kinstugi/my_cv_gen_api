using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

/// <summary>
/// Korina layout: light left sidebar (avatar, contact, education, skills, awards)
/// and right main column (name/title, profile, work experience, references).
/// </summary>
public class Template6 : ICvTemplate
{
    public void Compose(IDocumentContainer container, CvRenderModel model)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.PageColor("#e8e8e8");
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana).FontColor(Colors.Black));

            page.Content().AlignCenter().Column(root =>
            {
                root.Item().Background(Colors.White)
                    .Padding(0)
                    .Element(PageBody, model);
            });
        });
    }

    private static void PageBody(IContainer container, CvRenderModel model)
    {
        container.Row(row =>
        {
            // LEFT SIDEBAR
            row.ConstantItem(190)
                .Background(Colors.White)
                .BorderRight(1)
                .BorderColor("#dddddd")
                .PaddingVertical(30)
                .PaddingHorizontal(22)
                .Column(side =>
                {
                    // Avatar circle (no image for now, just initials)
                    side.Item().AlignCenter().Element(c =>
                    {
                        c.Width(90)
                         .Height(90)
                         .Border(1, "#cccccc")
                         .CornerRadius(45)
                         .Background("#cccccc")
                         .AlignCenter()
                         .AlignMiddle()
                         .Text(string.IsNullOrWhiteSpace(model.Name)
                             ? " "
                             : string.Join("", model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => s[0])))
                         .FontSize(26)
                         .FontColor("#666666");
                    });

                    side.Item().PaddingTop(20);

                    // Contact
                    side.Item().Text("Contact")
                        .FontSize(11)
                        .Bold()
                        .FontColor("#111111");

                    side.Item().PaddingTop(8).Column(c =>
                    {
                        if (!string.IsNullOrEmpty(model.Phone))
                            ContactRow(c, "📠", model.Phone);
                        if (!string.IsNullOrEmpty(model.Email))
                            ContactRow(c, "✉️", model.Email);
                        if (!string.IsNullOrEmpty(model.Location))
                            ContactRow(c, "📍", model.Location);
                        if (string.IsNullOrEmpty(model.Phone) &&
                            string.IsNullOrEmpty(model.Email) &&
                            string.IsNullOrEmpty(model.Location))
                        {
                            c.Item().Text("—").FontSize(9).FontColor("#777777");
                        }
                    });

                    // Education
                    if (model.Educations?.Count > 0)
                    {
                        side.Item().PaddingTop(18);
                        side.Item().Text("Education")
                            .FontSize(11)
                            .Bold()
                            .FontColor("#111111");

                        side.Item().PaddingTop(8).Column(c =>
                        {
                            foreach (var edu in model.Educations)
                            {
                                c.Item().Row(r =>
                                {
                                    r.ConstantItem(8).AlignTop().Text("⬤").FontSize(6).FontColor("#555555");
                                    r.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text(edu.School)
                                            .FontSize(9.5f)
                                            .FontColor("#333333");
                                        col.Item().Text($"{edu.Degree} in {edu.FieldOfStudy}")
                                            .FontSize(9)
                                            .FontColor("#555555")
                                            .Italic();
                                        var yearText = edu.EndDate?.ToString("yyyy") ?? edu.StartDate.ToString("yyyy");
                                        col.Item().Text(yearText)
                                            .FontSize(8.5f)
                                            .FontColor("#777777");
                                    });
                                });
                            }
                        });
                    }

                    // Skills
                    if (model.Skills?.Count > 0)
                    {
                        side.Item().PaddingTop(18);
                        side.Item().Text("Skill")
                            .FontSize(11)
                            .Bold()
                            .FontColor("#111111");

                        side.Item().PaddingTop(6).Column(c =>
                        {
                            foreach (var skill in model.Skills)
                            {
                                c.Item().Row(r =>
                                {
                                    r.ConstantItem(10).Text("•").FontSize(10).FontColor("#555555");
                                    r.RelativeItem().Text(skill).FontSize(9.5f).FontColor("#333333");
                                });
                            }
                        });
                    }

                    // Awards (we don't track awards separately in the model;
                    // you can encode them as part of projects or summary if desired.)
                });

            // RIGHT MAIN
            row.RelativeItem()
                .PaddingVertical(30)
                .PaddingHorizontal(32)
                .Column(main =>
                {
                    // Name + Title
                    main.Item().Column(header =>
                    {
                        header.Item().Text(model.Name)
                            .FontSize(24)
                            .ExtraBold()
                            .FontColor("#111111");

                        if (!string.IsNullOrEmpty(model.Title))
                        {
                            header.Item().Text(model.Title)
                                .FontSize(11)
                                .FontColor("#888888")
                                .LetterSpacing(2f)
                                .Caps();
                        }
                    });

                    // Divider
                    main.Item().PaddingTop(12)
                        .Height(1)
                        .Background("#e0e0e0");

                    // Profile (Summary)
                    if (!string.IsNullOrEmpty(model.Summary))
                    {
                        main.Item().PaddingTop(18);
                        main.Item().Text("Profile")
                            .FontSize(12)
                            .Bold()
                            .FontColor("#111111");

                        main.Item().PaddingTop(6)
                            .Text(model.Summary)
                            .FontSize(10.5f)
                            .FontColor("#555555")
                            .LineHeight(1.6f)
                            .AlignJustify();
                    }

                    // Work Experience
                    if (model.WorkExperiences?.Count > 0)
                    {
                        main.Item().PaddingTop(20);
                        main.Item().Text("Work Experience")
                            .FontSize(12)
                            .Bold()
                            .FontColor("#111111");

                        foreach (var exp in model.WorkExperiences)
                        {
                            var endStr = exp.IsCurrent || exp.EndDate == null
                                ? "Present"
                                : exp.EndDate.Value.ToString("yyyy");

                            main.Item().PaddingTop(10).Column(expCol =>
                            {
                                // Company | Role
                                expCol.Item().Text($"{exp.Company} | {exp.Position}")
                                    .FontSize(11.5f)
                                    .Bold()
                                    .FontColor("#111111");

                                // Years
                                expCol.Item().Text($"{exp.StartDate:yyyy} – {endStr}")
                                    .FontSize(10)
                                    .FontColor("#666666")
                                    .Italic();

                                // Description
                                if (!string.IsNullOrEmpty(exp.Description))
                                {
                                    var bullets = exp.Description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                                    expCol.Item().PaddingTop(4).Column(descCol =>
                                    {
                                        foreach (var bullet in bullets)
                                        {
                                            descCol.Item().Text(bullet.Trim())
                                                .FontSize(10)
                                                .FontColor("#555555")
                                                .LineHeight(1.6f);
                                        }
                                    });
                                }
                            });
                        }
                    }

                    // References: map from Languages or Projects if you later add a model for references.
                });
        });
    }

    private static void ContactRow(ColumnDescriptor column, string icon, string value)
    {
        column.Item().Row(r =>
        {
            r.ConstantItem(12).AlignTop().Text(icon).FontSize(10);
            r.RelativeItem().Text(value).FontSize(9.5f).FontColor("#333333");
        });
    }
}

