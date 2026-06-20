using System;
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RehabCenterApp.Models;

namespace RehabCenterApp.Services;

public class PrintService
{
    private readonly DatabaseService _dbService;

    public PrintService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<string> GenerateReceiptAsync(Payment payment, string centerName, string centerPhone)
    {
        var fileName = $"Receipt_{payment.ReceiptNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var document = new Document(PageSize.A5, 20, 20, 20, 20);
        using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
        document.Open();

        // Header
        var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD, new BaseColor(64, 64, 64));
        var headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
        var normalFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);
        var boldFont = FontFactory.GetFont("Arial", 10, Font.BOLD);

        document.Add(new Paragraph(centerName, titleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph($"Tel: {centerPhone}", normalFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph(" ", normalFont));
        document.Add(new Paragraph("Payment Receipt", headerFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph(" ", normalFont));

        // Receipt details table
        var table = new PdfPTable(2) { WidthPercentage = 100 };
        table.AddCell(new PdfPCell(new Phrase("Receipt No:", boldFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase(payment.ReceiptNumber, normalFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase("Date:", boldFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase(payment.Date.ToString("yyyy-MM-dd"), normalFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase("Patient:", boldFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase(payment.Beneficiary?.Name ?? "", normalFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase("Amount:", boldFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase($"{payment.Amount:C}", boldFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase("Payment Type:", boldFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase(payment.PaymentType, normalFont)) { Border = Rectangle.NO_BORDER });
        document.Add(table);

        document.Add(new Paragraph(" ", normalFont));
        document.Add(new Paragraph("Thank you for your payment!", normalFont) { Alignment = Element.ALIGN_CENTER });

        document.Close();
        return filePath;
    }

    public async Task<string> ExportBeneficiariesToPdfAsync(System.Collections.Generic.List<Beneficiary> beneficiaries)
    {
        var fileName = $"Beneficiaries_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var document = new Document(PageSize.A4, 20, 20, 20, 20);
        using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
        document.Open();

        var titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
        var headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, new BaseColor(255, 255, 255));
        var normalFont = FontFactory.GetFont("Arial", 9, Font.NORMAL);

        document.Add(new Paragraph("Beneficiaries Report", titleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph(" ", normalFont));

        var table = new PdfPTable(6) { WidthPercentage = 100 };
        var headerBg = new BaseColor(30, 58, 95);

        string[] headers = { "Name", "Age", "Disability", "Phone", "Status", "Registration" };
        foreach (var h in headers)
        {
            var cell = new PdfPCell(new Phrase(h, headerFont)) { BackgroundColor = headerBg, Padding = 5 };
            table.AddCell(cell);
        }

        foreach (var b in beneficiaries)
        {
            table.AddCell(new PdfPCell(new Phrase(b.Name, normalFont)));
            table.AddCell(new PdfPCell(new Phrase(b.Age.ToString(), normalFont)));
            table.AddCell(new PdfPCell(new Phrase(b.DisabilityType, normalFont)));
            table.AddCell(new PdfPCell(new Phrase(b.Phone ?? "", normalFont)));
            table.AddCell(new PdfPCell(new Phrase(b.Status, normalFont)));
            table.AddCell(new PdfPCell(new Phrase(b.RegistrationDate.ToString("yyyy-MM-dd"), normalFont)));
        }

        document.Add(table);
        document.Close();
        return filePath;
    }

    public async Task<string> ExportDailyScheduleToPdfAsync(System.Collections.Generic.List<Session> sessions, DateTime date, string centerName)
    {
        var fileName = $"DailySchedule_{date:yyyyMMdd}.pdf";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var document = new Document(PageSize.A4, 30, 30, 40, 40);
        using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
        document.Open();

        var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD, new BaseColor(30, 58, 95));
        var subTitleFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, new BaseColor(80, 80, 80));
        var headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, new BaseColor(255, 255, 255));
        var normalFont = FontFactory.GetFont("Arial", 9, Font.NORMAL);
        var boldFont = FontFactory.GetFont("Arial", 9, Font.BOLD);
        var headerBg = new BaseColor(30, 58, 95);
        var altRowBg = new BaseColor(245, 248, 255);

        document.Add(new Paragraph(centerName, titleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph("Daily Therapist Schedule", subTitleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph($"Date: {date:dddd, MMMM dd, yyyy}  |  Total Sessions: {sessions.Count}", normalFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph(" ", normalFont));

        // Summary by therapist
        var byTherapist = new System.Collections.Generic.Dictionary<string, int>();
        foreach (var s in sessions)
        {
            var tName = s.Therapist?.Name ?? "Unknown";
            byTherapist[tName] = byTherapist.TryGetValue(tName, out var cnt) ? cnt + 1 : 1;
        }

        document.Add(new Paragraph("Summary by Therapist:", boldFont));
        var summaryTable = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT };
        summaryTable.SetWidths(new float[] { 3, 1 });
        summaryTable.AddCell(new PdfPCell(new Phrase("Therapist", headerFont)) { BackgroundColor = headerBg, Padding = 4 });
        summaryTable.AddCell(new PdfPCell(new Phrase("Sessions", headerFont)) { BackgroundColor = headerBg, Padding = 4 });
        foreach (var kvp in byTherapist)
        {
            summaryTable.AddCell(new PdfPCell(new Phrase(kvp.Key, normalFont)) { Padding = 4 });
            summaryTable.AddCell(new PdfPCell(new Phrase(kvp.Value.ToString(), normalFont)) { Padding = 4, HorizontalAlignment = Element.ALIGN_CENTER });
        }
        document.Add(summaryTable);
        document.Add(new Paragraph(" ", normalFont));

        // Main schedule table
        document.Add(new Paragraph("Full Schedule:", boldFont));
        document.Add(new Paragraph(" ", normalFont));
        var table = new PdfPTable(6) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 1f, 1.6f, 2f, 2f, 1f, 1.2f });

        string[] cols = { "Time", "Beneficiary", "Session Type", "Therapist", "Duration", "Status" };
        foreach (var h in cols)
        {
            var cell = new PdfPCell(new Phrase(h, headerFont)) { BackgroundColor = headerBg, Padding = 6 };
            table.AddCell(cell);
        }

        bool alt = false;
        foreach (var s in sessions)
        {
            var bg = alt ? altRowBg : new BaseColor(255, 255, 255);
            AddScheduleCell(table, s.Time.ToString(@"hh\:mm"), normalFont, bg);
            AddScheduleCell(table, s.Beneficiary?.Name ?? "", normalFont, bg);
            AddScheduleCell(table, s.SessionType, normalFont, bg);
            AddScheduleCell(table, s.Therapist?.Name ?? "", normalFont, bg);
            AddScheduleCell(table, $"{s.Duration} min", normalFont, bg);
            AddScheduleCell(table, s.Status, normalFont, bg);
            alt = !alt;
        }

        document.Add(table);
        document.Add(new Paragraph(" ", normalFont));
        document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont) { Alignment = Element.ALIGN_RIGHT });

        document.Close();
        return filePath;
    }

    private static void AddScheduleCell(PdfPTable table, string text, Font font, BaseColor bg)
    {
        table.AddCell(new PdfPCell(new Phrase(text, font)) { BackgroundColor = bg, Padding = 5 });
    }

    public async Task<string> ExportChildHistoryToPdfAsync(Beneficiary beneficiary, System.Collections.Generic.List<Session> sessions, System.Collections.Generic.List<TherapistReport> reports)
    {
        var centerName = await _dbService.GetSettingAsync("CenterName") ?? "Rehab Center";
        var fileName = $"ChildHistory_{beneficiary.Name}_{DateTime.Now:yyyyMMdd}.pdf";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var document = new Document(PageSize.A4, 30, 30, 40, 40);
        using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
        document.Open();

        var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD, new BaseColor(30, 58, 95));
        var headingFont = FontFactory.GetFont("Arial", 13, Font.BOLD, new BaseColor(30, 58, 95));
        var subHeadFont = FontFactory.GetFont("Arial", 11, Font.BOLD, new BaseColor(60, 60, 60));
        var headerFont = FontFactory.GetFont("Arial", 9, Font.BOLD, new BaseColor(255, 255, 255));
        var normalFont = FontFactory.GetFont("Arial", 9, Font.NORMAL);
        var boldFont = FontFactory.GetFont("Arial", 9, Font.BOLD);
        var headerBg = new BaseColor(30, 58, 95);
        var sectionBg = new BaseColor(235, 242, 255);

        document.Add(new Paragraph(centerName, titleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph("Child Progress History Report", headingFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph(" ", normalFont));

        // Child info
        var infoTable = new PdfPTable(4) { WidthPercentage = 100 };
        infoTable.SetWidths(new float[] { 1.2f, 2f, 1.2f, 2f });
        void AddInfoRow(string l1, string v1, string l2, string v2)
        {
            infoTable.AddCell(new PdfPCell(new Phrase(l1, boldFont)) { BackgroundColor = sectionBg, Padding = 5, Border = Rectangle.BOX });
            infoTable.AddCell(new PdfPCell(new Phrase(v1, normalFont)) { Padding = 5, Border = Rectangle.BOX });
            infoTable.AddCell(new PdfPCell(new Phrase(l2, boldFont)) { BackgroundColor = sectionBg, Padding = 5, Border = Rectangle.BOX });
            infoTable.AddCell(new PdfPCell(new Phrase(v2, normalFont)) { Padding = 5, Border = Rectangle.BOX });
        }
        AddInfoRow("Name:", beneficiary.Name, "Date of Birth:", beneficiary.DateOfBirth.ToString("yyyy-MM-dd"));
        AddInfoRow("Disability:", beneficiary.DisabilityType ?? "", "Diagnosis:", beneficiary.Diagnosis ?? "");
        AddInfoRow("Guardian:", beneficiary.GuardianName ?? "", "Phone:", beneficiary.GuardianPhone ?? "");
        AddInfoRow("Total Sessions:", sessions.Count.ToString(), "Reports:", reports.Count.ToString());
        document.Add(infoTable);
        document.Add(new Paragraph(" ", normalFont));

        // Sessions
        document.Add(new Paragraph("Session History:", subHeadFont));
        document.Add(new Paragraph(" ", normalFont));
        var sessionTable = new PdfPTable(5) { WidthPercentage = 100 };
        sessionTable.SetWidths(new float[] { 1.2f, 1.8f, 2f, 1f, 1f });
        string[] sCols = { "Date", "Session Type", "Therapist", "Duration", "Status" };
        foreach (var h in sCols)
            sessionTable.AddCell(new PdfPCell(new Phrase(h, headerFont)) { BackgroundColor = headerBg, Padding = 5 });

        bool alt = false;
        foreach (var s in sessions)
        {
            var bg = alt ? sectionBg : new BaseColor(255, 255, 255);
            AddScheduleCell(sessionTable, s.Date.ToString("yyyy-MM-dd"), normalFont, bg);
            AddScheduleCell(sessionTable, s.SessionType, normalFont, bg);
            AddScheduleCell(sessionTable, s.Therapist?.Name ?? "", normalFont, bg);
            AddScheduleCell(sessionTable, $"{s.Duration} min", normalFont, bg);
            AddScheduleCell(sessionTable, s.Status, normalFont, bg);
            alt = !alt;
        }
        document.Add(sessionTable);

        if (reports.Count > 0)
        {
            document.Add(new Paragraph(" ", normalFont));
            document.Add(new Paragraph("Session Reports:", subHeadFont));
            document.Add(new Paragraph(" ", normalFont));

            foreach (var r in reports)
            {
                document.Add(new Paragraph($"Report Date: {r.ReportDate:yyyy-MM-dd}  |  Rating: {r.OverallRating}/5 ★", boldFont));
                if (!string.IsNullOrWhiteSpace(r.ActivitiesPerformed))
                    document.Add(new Paragraph($"Activities: {r.ActivitiesPerformed}", normalFont));
                if (!string.IsNullOrWhiteSpace(r.ChildResponse))
                    document.Add(new Paragraph($"Child Response: {r.ChildResponse}", normalFont));
                if (!string.IsNullOrWhiteSpace(r.GoalsAddressed))
                    document.Add(new Paragraph($"Goals Addressed: {r.GoalsAddressed}", normalFont));
                if (!string.IsNullOrWhiteSpace(r.BehaviorNotes))
                    document.Add(new Paragraph($"Behavior Notes: {r.BehaviorNotes}", normalFont));
                if (!string.IsNullOrWhiteSpace(r.Homework))
                    document.Add(new Paragraph($"Homework: {r.Homework}", normalFont));
                if (!string.IsNullOrWhiteSpace(r.NextSessionPlan))
                    document.Add(new Paragraph($"Next Session Plan: {r.NextSessionPlan}", normalFont));
                document.Add(new Paragraph("─────────────────────────────────────────", normalFont));
            }
        }

        document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont) { Alignment = Element.ALIGN_RIGHT });
        document.Close();
        return filePath;
    }
}