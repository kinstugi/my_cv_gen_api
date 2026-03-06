using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

/// <summary>
/// Olivia layout: two equal-width white columns inside a grey background.
/// Left: photo, name/title, work experience, references.
/// Right: contacts, about (summary), education, skills.
/// </summary>
public class Template7 : ICvTemplate
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
                root.Item().Row(card =>
                {
                    card.RelativeItem().Background(Colors.White).Element(c => BuildLeftColumn(c, model));
                    card.RelativeItem().Background(Colors.White).Element(c => BuildRightColumn(c, model));
                });
            });
        });
    }

    private static void BuildLeftColumn(IContainer container, CvRenderModel model)
    {
        container
            .PaddingLeft(30)
            .PaddingRight(26)
            .PaddingVertical(32)
            .BorderRight(1)
            .BorderColor("#e5e5e5")
            .Column(col =>
            {
                // Photo (placeholder circle)
                col.Item().Element(c =>
                {
                    c.Width(100)
                     .Height(100)
                     .CornerRadius(50)
                     .Background("#dddddd")
                     .AlignCenter()
                     .AlignMiddle()
                     .Text("👤")
                     .FontSize(40)
                     .FontColor("#999999");
                });

                // Name + Title
                col.Item().PaddingTop(12).Column(h =>
                {
                    h.Item().Text(model.Name)
                        .FontSize(22)
                        .ExtraBold()
                        .FontFamily(Fonts.TimesNewRoman)
                        .FontColor("#111111");

                    if (!string.IsNullOrEmpty(model.Title))
                    {
                        h.Item().PaddingTop(2).Text(model.Title)
                            .FontSize(11)
                            .FontColor("#666666");
                    }
                });

                // Work Experience
                if (model.WorkExperiences?.Count > 0)
                {
                    col.Item().PaddingTop(24).Element(c => SectionHeader(c, "Work Experience"));

                    foreach (var exp in model.WorkExperiences)
                    {
                        var endStr = exp.IsCurrent || exp.EndDate == null
                            ? "Present"
                            : exp.EndDate.Value.ToString("yyyy");

                        col.Item().PaddingTop(10).Column(expCol =>
                        {
                            expCol.Item().Row(r =>
                            {
                                r.RelativeItem().Text(exp.Position)
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor("#111111");
                                r.AutoItem().Text($"{exp.StartDate:yyyy} – {endStr}")
                                    .FontSize(9)
                                    .FontColor("#888888");
                            });

                            expCol.Item().Text(exp.Company)
                                .FontSize(10)
                                .Bold()
                                .FontColor("#444444");

                            if (!string.IsNullOrEmpty(exp.Description))
                            {
                                expCol.Item().PaddingTop(2)
                                    .Text(exp.Description)
                                    .FontSize(9.5f)
                                    .FontColor("#666666")
                                    .LineHeight(1.5f);
                            }
                        });
                    }
                }

                // References: not modeled in CvRenderModel; omit rather than fabricate.
            });
    }

    private static void BuildRightColumn(IContainer container, CvRenderModel model)
    {
        container
            .PaddingLeft(26)
            .PaddingRight(30)
            .PaddingVertical(32)
            .Column(col =>
            {
                // Contacts (from Phone, Email, Website, Location)
                col.Item().Element(c => SectionHeader(c, "Contacts"));

                col.Item().PaddingTop(4).Column(contactCol =>
                {
                    if (!string.IsNullOrEmpty(model.Phone))
                        ContactPill(contactCol, "📞", model.Phone);
                    if (!string.IsNullOrEmpty(model.Email))
                        ContactPill(contactCol, "✉️", model.Email);
                    if (!string.IsNullOrEmpty(model.Website))
                        ContactPill(contactCol, "🌐", model.Website);
                    if (!string.IsNullOrEmpty(model.Location))
                        ContactPill(contactCol, "📍", model.Location);
                });

                // About Me (Summary)
                if (!string.IsNullOrEmpty(model.Summary))
                {
                    col.Item().PaddingTop(18).Element(c => SectionHeader(c, "About Me"));
                    col.Item().PaddingTop(4)
                        .Text(model.Summary)
                        .FontSize(9.5f)
                        .FontColor("#555555")
                        .LineHeight(1.6f)
                        .AlignJustify();
                }

                // Education
                if (model.Educations?.Count > 0)
                {
                    col.Item().PaddingTop(18).Element(c => SectionHeader(c, "Education"));

                    foreach (var edu in model.Educations)
                    {
                        var years = $"{edu.StartDate:yyyy} - {(edu.EndDate?.ToString("yyyy") ?? "—")}";

                        col.Item().PaddingTop(6).Column(eduCol =>
                        {
                            eduCol.Item().Text($"{edu.Degree} in {edu.FieldOfStudy}")
                                .FontSize(10)
                                .Bold()
                                .FontColor("#111111");
                            eduCol.Item().Text(edu.School)
                                .FontSize(9.5f)
                                .FontColor("#555555");
                            eduCol.Item().Text(years)
                                .FontSize(8.5f)
                                .FontColor("#999999");
                        });
                    }
                }

                // Skills
                if (model.Skills?.Count > 0)
                {
                    col.Item().PaddingTop(18).Element(c => SectionHeader(c, "Skills"));

                    col.Item().PaddingTop(4).Column(skillCol =>
                    {
                        foreach (var skill in model.Skills)
                        {
                            skillCol.Item().Row(r =>
                            {
                                r.ConstantItem(10).Text("•").FontSize(11).FontColor("#555555");
                                r.RelativeItem().Text(skill).FontSize(9.5f).FontColor("#333333");
                            });
                        }
                    });
                }
            });
    }

    private static void SectionHeader(IContainer container, string title)
    {
        container.Text(title)
            .FontFamily(Fonts.TimesNewRoman)
            .FontSize(12)
            .Bold()
            .FontColor("#111111")
            .PaddingBottom(4)
            .BorderBottom(1.5f)
            .BorderColor("#cccccc");
    }

    private static void ContactPill(ColumnDescriptor col, string icon, string value)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(22).Element(c =>
            {
                c.Width(20)
                 .Height(20)
                 .CornerRadius(10)
                 .Background("#222222")
                 .AlignCenter()
                 .AlignMiddle()
                 .Text(icon)
                 .FontSize(9)
                 .FontColor(Colors.White);
            });

            row.RelativeItem().Text(value)
                .FontSize(9.5f)
                .FontColor("#444444");
        });
    }
}

