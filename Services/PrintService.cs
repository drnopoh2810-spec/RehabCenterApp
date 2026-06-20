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
        var headerBg = new BaseColor(30, 58, 95); // #1e3a5f

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
}