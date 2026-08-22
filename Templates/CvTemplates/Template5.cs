using my_cv_gen_api.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace my_cv_gen_api.Templates.CvTemplates;

/// <summary>
/// Modern black-and-white layout matching the frontend ModernBwTemplate preview.
/// </summary>
public class Template5 : ICvTemplate
{
    private const string SidebarBg = "#2d3748";
    private const string SidebarMuted = "#cbd5e0";
    private const string SidebarSecondary = "#a0aec0";
    private const string Ink = "#1a202c";
    private const string Body = "#4a5568";
    private const string Muted = "#718096";
    private const string Rule = "#e2e8f0";

    public void Compose(IDocumentContainer container, CvRenderModel model)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.PageColor("#f7fafc");
            page.DefaultTextStyle(style => style
                .FontFamily(Fonts.TimesNewRoman)
                .FontSize(10)
                .FontColor(Body));

            page.Content().Background(Colors.White).Row(row =>
            {
                row.ConstantItem(170)
                    .Background(SidebarBg)
                    .PaddingVertical(26)
                    .PaddingHorizontal(18)
                    .Column(sidebar => BuildSidebar(sidebar, model));

                row.RelativeItem()
                    .Padding(26)
                    .Column(main => BuildMain(main, model));
            });
        });
    }

    private static void BuildSidebar(ColumnDescriptor sidebar, CvRenderModel model)
    {
        sidebar.Item().AlignCenter().Element(container =>
        {
            container.Width(82).Height(82)
                .Border(3).BorderColor("#4a5568")
                .Background("#4a5568")
                .AlignCenter().AlignMiddle()
                .Text(Initials(model.Name))
                .FontSize(28).FontColor(SidebarSecondary).Bold();
        });

        sidebar.Item().PaddingTop(24).Element(c => SidebarHeading(c, "Contact"));
        AddContact(sidebar, "Location", model.Location);
        AddContact(sidebar, "Phone", model.Phone);
        AddContact(sidebar, "Email", model.Email);
        AddContact(sidebar, "GitHub", model.GitHubUrl);
        AddContact(sidebar, "Website", model.Website);

        if (model.Educations.Count > 0)
        {
            sidebar.Item().PaddingTop(18).Element(c => SidebarHeading(c, "Education"));
            foreach (var education in model.Educations)
            {
                sidebar.Item().PaddingTop(8).Column(item =>
                {
                    item.Item().Text(DateRange(education.StartDate, education.EndDate))
                        .FontSize(8.5f).FontColor(SidebarSecondary);
                    item.Item().Text(JoinEducation(education))
                        .FontSize(10).Bold().FontColor(Colors.White);
                    item.Item().Text(education.School)
                        .FontSize(9).FontColor(SidebarMuted);
                });
            }
        }

        if (model.Skills.Count > 0)
        {
            sidebar.Item().PaddingTop(18).Element(c => SidebarHeading(c, "Skills"));
            sidebar.Item().PaddingTop(7).Column(list =>
            {
                foreach (var skill in model.Skills)
                    list.Item().Row(row =>
                    {
                        row.ConstantItem(9).Text("•").FontSize(10).FontColor("#718096");
                        row.RelativeItem().Text(skill).FontSize(9).FontColor(SidebarMuted);
                    });
            });
        }

        if (model.Languages.Count > 0)
        {
            sidebar.Item().PaddingTop(18).Element(c => SidebarHeading(c, "Language"));
            sidebar.Item().PaddingTop(7).Column(list =>
            {
                foreach (var language in model.Languages)
                    list.Item().Row(row =>
                    {
                        row.ConstantItem(9).Text("•").FontSize(10).FontColor("#718096");
                        row.RelativeItem().Text(LanguageText(language))
                            .FontSize(9).Bold().FontColor(Colors.White);
                    });
            });
        }
    }

    private static void BuildMain(ColumnDescriptor main, CvRenderModel model)
    {
        main.Item().PaddingBottom(18).Column(header =>
        {
            header.Item().Text(string.IsNullOrWhiteSpace(model.Name) ? "Your name" : model.Name)
                .FontSize(27).Bold().FontColor(Ink);
            if (!string.IsNullOrWhiteSpace(model.Title))
                header.Item().PaddingTop(3).Text(model.Title.ToUpperInvariant())
                    .FontSize(10).LetterSpacing(3f).FontColor(Muted);
            if (!string.IsNullOrWhiteSpace(model.Summary))
                header.Item().PaddingTop(9).Text(model.Summary)
                    .FontSize(9.5f).LineHeight(1.7f).FontColor(Body);
        });

        if (model.WorkExperiences.Count > 0)
        {
            main.Item().Element(c => MainHeading(c, "Experience"));
            foreach (var experience in model.WorkExperiences)
            {
                main.Item().PaddingTop(12).Row(row =>
                {
                    row.ConstantItem(18).AlignTop().Element(dot =>
                    {
                        dot.Width(11).Height(11).Border(2).BorderColor("#a0aec0")
                            .Background(Colors.White);
                    });

                    row.RelativeItem().Column(item =>
                    {
                        item.Item().Text(DateRange(experience.StartDate, experience.EndDate, experience.IsCurrent))
                            .FontSize(9).FontColor(Muted);
                        item.Item().PaddingTop(2).Text(experience.Company)
                            .FontSize(9.5f).FontColor(Muted);
                        item.Item().Text(experience.Position)
                            .FontSize(11).Bold().FontColor(Ink);
                        AddBullets(item, experience.Description, 9.5f);
                    });
                });
            }
        }

        if (model.Projects.Count > 0)
        {
            main.Item().PaddingTop(22).Element(c => MainHeading(c, "Projects"));
            foreach (var project in model.Projects)
            {
                main.Item().PaddingTop(10).Column(item =>
                {
                    item.Item().Row(titleRow =>
                    {
                        titleRow.RelativeItem().Text(project.Title)
                            .FontSize(10.5f).Bold().FontColor(Ink);
                        if (!string.IsNullOrWhiteSpace(project.Link))
                            titleRow.AutoItem().Text(project.Link)
                                .FontSize(8.5f).FontColor("#2563eb").Underline();
                    });
                    if (!string.IsNullOrWhiteSpace(project.Description))
                        item.Item().PaddingTop(3).Text(project.Description)
                            .FontSize(9.5f).LineHeight(1.7f).FontColor(Body);
                });
            }
        }
    }

    private static void SidebarHeading(IContainer container, string text)
    {
        container.BorderBottom(1).BorderColor("#4a5568").PaddingBottom(5)
            .Text(text).FontSize(12).Bold().FontColor(Colors.White);
    }

    private static void MainHeading(IContainer container, string text)
    {
        container.BorderBottom(1.5f).BorderColor(Rule).PaddingBottom(5)
            .Text(text).FontSize(15).Bold().FontColor(Ink);
    }

    private static void AddContact(ColumnDescriptor column, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        column.Item().PaddingTop(7).Column(item =>
        {
            item.Item().Text(label.ToUpperInvariant()).FontSize(8.5f).Bold()
                .LetterSpacing(1f).FontColor(Colors.White);
            item.Item().Text(value).FontSize(9).FontColor(SidebarMuted);
        });
    }

    private static void AddBullets(ColumnDescriptor column, string description, float fontSize)
    {
        var bullets = description.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (bullets.Length == 0) return;
        column.Item().PaddingTop(4).Column(list =>
        {
            foreach (var bullet in bullets)
                list.Item().Row(row =>
                {
                    row.ConstantItem(10).Text("•").FontSize(fontSize).FontColor(Body);
                    row.RelativeItem().Text(bullet).FontSize(fontSize).LineHeight(1.7f).FontColor(Body);
                });
        });
    }

    private static string Initials(string name)
    {
        var initials = string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2).Select(part => part[0].ToString().ToUpperInvariant()));
        return string.IsNullOrWhiteSpace(initials) ? " " : initials;
    }

    private static string DateRange(DateTime start, DateTime? end, bool current = false)
        => $"{start:yyyy-MM-dd} - {(current || end is null ? "Present" : end.Value.ToString("yyyy-MM-dd"))}";

    private static string JoinEducation(CvEducation education)
        => string.Join(" in ", new[] { education.Degree, education.FieldOfStudy }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string LanguageText(CvLanguage language)
        => string.IsNullOrWhiteSpace(language.Level) ? language.Name : $"{language.Name} ({language.Level})";
}
