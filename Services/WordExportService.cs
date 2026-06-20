using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using RehabCenterApp.Models;

namespace RehabCenterApp.Services;

public class WordExportService
{
    private readonly DatabaseService _dbService;

    public WordExportService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<string> ExportDailyReportAsync(TherapistReport report)
    {
        var centerName = await _dbService.GetSettingAsync("CenterName") ?? "Rehab Center";
        var fileName = $"Report_{report.Session?.Beneficiary?.Name ?? "Child"}_{report.ReportDate:yyyyMMdd}.docx";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddStylesPart(mainPart);

        AddHeading(body, centerName, "Heading1", true);
        AddHeading(body, "Daily Session Report", "Heading2", true);

        AddSeparator(body);

        var infoTable = CreateTable(2, false);
        AddTableRow(infoTable, "Beneficiary:", report.Session?.Beneficiary?.Name ?? "");
        AddTableRow(infoTable, "Session Type:", report.Session?.SessionType ?? "");
        AddTableRow(infoTable, "Therapist:", report.Therapist?.Name ?? "");
        AddTableRow(infoTable, "Date:", report.ReportDate.ToString("yyyy-MM-dd"));
        AddTableRow(infoTable, "Duration:", $"{report.Session?.Duration ?? 0} minutes");
        AddTableRow(infoTable, "Overall Rating:", string.Concat(Enumerable.Repeat("★", report.OverallRating)) + string.Concat(Enumerable.Repeat("☆", 5 - report.OverallRating)));
        body.Append(infoTable);

        AddParagraph(body, "");

        AddSection(body, "Activities Performed", report.ActivitiesPerformed);
        AddSection(body, "Child Response", report.ChildResponse);
        AddSection(body, "Goals Addressed", report.GoalsAddressed);
        AddSection(body, "Behavior Notes", report.BehaviorNotes);
        AddSection(body, "Homework / Parent Instructions", report.Homework);
        AddSection(body, "Next Session Plan", report.NextSessionPlan);

        AddParagraph(body, "");
        AddParagraph(body, $"Report generated: {DateTime.Now:yyyy-MM-dd HH:mm}");

        mainPart.Document.Save();
        return filePath;
    }

    public async Task<string> ExportChildHistoryAsync(Beneficiary beneficiary, List<Session> sessions, List<TherapistReport> reports)
    {
        var centerName = await _dbService.GetSettingAsync("CenterName") ?? "Rehab Center";
        var fileName = $"History_{beneficiary.Name}_{DateTime.Now:yyyyMMdd}.docx";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddStylesPart(mainPart);

        AddHeading(body, centerName, "Heading1", true);
        AddHeading(body, "Child Progress History", "Heading2", true);
        AddSeparator(body);

        var infoTable = CreateTable(2, false);
        AddTableRow(infoTable, "Name:", beneficiary.Name);
        AddTableRow(infoTable, "Date of Birth:", beneficiary.DateOfBirth.ToString("yyyy-MM-dd"));
        AddTableRow(infoTable, "Disability Type:", beneficiary.DisabilityType ?? "");
        AddTableRow(infoTable, "Diagnosis:", beneficiary.Diagnosis ?? "");
        AddTableRow(infoTable, "Guardian:", beneficiary.GuardianName ?? "");
        AddTableRow(infoTable, "Guardian Phone:", beneficiary.GuardianPhone ?? "");
        AddTableRow(infoTable, "Total Sessions:", sessions.Count.ToString());
        if (sessions.Count > 0)
        {
            AddTableRow(infoTable, "First Session:", sessions.Min(s => s.Date).ToString("yyyy-MM-dd"));
            AddTableRow(infoTable, "Last Session:", sessions.Max(s => s.Date).ToString("yyyy-MM-dd"));
        }
        body.Append(infoTable);

        AddParagraph(body, "");
        AddHeading(body, "Session History", "Heading2", false);

        var sessionsByDate = sessions.OrderBy(s => s.Date).ToList();
        foreach (var session in sessionsByDate)
        {
            var report = reports.FirstOrDefault(r => r.SessionId == session.Id);

            AddHeading(body, $"{session.Date:yyyy-MM-dd} | {session.SessionType} | {session.Therapist?.Name}", "Heading3", false);

            var sessionTable = CreateTable(2, false);
            AddTableRow(sessionTable, "Time:", session.Time.ToString(@"hh\:mm"));
            AddTableRow(sessionTable, "Duration:", $"{session.Duration} min");
            AddTableRow(sessionTable, "Status:", session.Status);
            if (!string.IsNullOrWhiteSpace(session.Notes))
                AddTableRow(sessionTable, "Session Notes:", session.Notes);
            body.Append(sessionTable);

            if (report != null)
            {
                AddParagraph(body, "Session Report:", bold: true);
                if (!string.IsNullOrWhiteSpace(report.ActivitiesPerformed))
                    AddLabeledParagraph(body, "Activities:", report.ActivitiesPerformed);
                if (!string.IsNullOrWhiteSpace(report.ChildResponse))
                    AddLabeledParagraph(body, "Child Response:", report.ChildResponse);
                if (!string.IsNullOrWhiteSpace(report.GoalsAddressed))
                    AddLabeledParagraph(body, "Goals Addressed:", report.GoalsAddressed);
                if (!string.IsNullOrWhiteSpace(report.Homework))
                    AddLabeledParagraph(body, "Homework:", report.Homework);
                if (!string.IsNullOrWhiteSpace(report.BehaviorNotes))
                    AddLabeledParagraph(body, "Behavior Notes:", report.BehaviorNotes);
                AddLabeledParagraph(body, "Rating:", string.Concat(Enumerable.Repeat("★", report.OverallRating)));
            }

            AddSeparator(body);
        }

        AddParagraph(body, $"Report generated: {DateTime.Now:yyyy-MM-dd HH:mm}");

        mainPart.Document.Save();
        return filePath;
    }

    private static void AddStylesPart(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles(
            CreateStyle("Heading1", "Heading 1", 32, true, "2E4057"),
            CreateStyle("Heading2", "Heading 2", 24, true, "1a6ea0"),
            CreateStyle("Heading3", "Heading 3", 20, true, "444444"),
            CreateStyle("Normal", "Normal", 20, false, "222222")
        );
        stylesPart.Styles.Save();
    }

    private static Style CreateStyle(string styleId, string styleName, int fontSize, bool bold, string colorHex)
    {
        return new Style(
            new StyleName { Val = styleName },
            new StyleRunProperties(
                new Bold { Val = bold ? OnOffValue.FromBoolean(true) : OnOffValue.FromBoolean(false) },
                new Color { Val = colorHex },
                new FontSize { Val = fontSize.ToString() },
                new FontSizeComplexScript { Val = fontSize.ToString() }
            )
        )
        { Type = StyleValues.Paragraph, StyleId = styleId };
    }

    private static void AddHeading(Body body, string text, string styleId, bool center)
    {
        var para = new Paragraph(
            new ParagraphProperties(
                new ParagraphStyleId { Val = styleId },
                new Justification { Val = center ? JustificationValues.Center : JustificationValues.Start }
            ),
            new Run(new Text(text))
        );
        body.Append(para);
    }

    private static void AddParagraph(Body body, string text, bool bold = false)
    {
        var runProps = bold ? new RunProperties(new Bold()) : new RunProperties();
        var para = new Paragraph(new Run(runProps, new Text(text)));
        body.Append(para);
    }

    private static void AddLabeledParagraph(Body body, string label, string value)
    {
        var para = new Paragraph(
            new Run(new RunProperties(new Bold()), new Text(label + " ")),
            new Run(new Text(value))
        );
        body.Append(para);
    }

    private static void AddSection(Body body, string title, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        AddParagraph(body, title + ":", bold: true);
        AddParagraph(body, content);
        AddParagraph(body, "");
    }

    private static void AddSeparator(Body body)
    {
        var para = new Paragraph(
            new ParagraphProperties(
                new ParagraphBorders(
                    new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "cccccc" }
                )
            )
        );
        body.Append(para);
    }

    private static Table CreateTable(int cols, bool hasHeader)
    {
        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.None },
                new BottomBorder { Val = BorderValues.None },
                new LeftBorder { Val = BorderValues.None },
                new RightBorder { Val = BorderValues.None },
                new InsideHorizontalBorder { Val = BorderValues.Single, Color = "e0e0e0", Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.None }
            ),
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
        ));
        return table;
    }

    private static void AddTableRow(Table table, string label, string value)
    {
        var row = new TableRow(
            new TableCell(
                new TableCellProperties(new TableCellWidth { Width = "1800", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(new Run(new RunProperties(new Bold()), new Text(label)))
            ),
            new TableCell(
                new Paragraph(new Run(new Text(value)))
            )
        );
        table.Append(row);
    }
}
