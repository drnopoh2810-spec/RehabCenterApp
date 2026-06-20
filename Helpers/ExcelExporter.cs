using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using RehabCenterApp.Models;

namespace RehabCenterApp.Helpers;

public static class ExcelExporter
{
    static ExcelExporter()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public static async Task<string> ExportBeneficiariesAsync(List<Beneficiary> beneficiaries)
    {
        var fileName = $"Beneficiaries_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Beneficiaries");

        string[] headers = { "Name", "Age", "Gender", "Disability Type", "Phone", "Guardian", "Status", "Registration Date" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(30, 58, 95));
            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        for (int i = 0; i < beneficiaries.Count; i++)
        {
            var b = beneficiaries[i];
            worksheet.Cells[i + 2, 1].Value = b.Name;
            worksheet.Cells[i + 2, 2].Value = b.Age;
            worksheet.Cells[i + 2, 3].Value = b.Gender;
            worksheet.Cells[i + 2, 4].Value = b.DisabilityType;
            worksheet.Cells[i + 2, 5].Value = b.Phone;
            worksheet.Cells[i + 2, 6].Value = b.GuardianName;
            worksheet.Cells[i + 2, 7].Value = b.Status;
            worksheet.Cells[i + 2, 8].Value = b.RegistrationDate.ToString("yyyy-MM-dd");
        }

        worksheet.Cells.AutoFitColumns();
        await package.SaveAsAsync(new FileInfo(filePath));
        return filePath;
    }

    public static async Task<string> ExportPaymentsAsync(List<Payment> payments)
    {
        var fileName = $"Payments_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Payments");

        string[] headers = { "Receipt No", "Beneficiary", "Amount", "Date", "Type", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        for (int i = 0; i < payments.Count; i++)
        {
            var p = payments[i];
            worksheet.Cells[i + 2, 1].Value = p.ReceiptNumber;
            worksheet.Cells[i + 2, 2].Value = p.Beneficiary?.Name;
            worksheet.Cells[i + 2, 3].Value = p.Amount;
            worksheet.Cells[i + 2, 4].Value = p.Date.ToString("yyyy-MM-dd");
            worksheet.Cells[i + 2, 5].Value = p.PaymentType;
            worksheet.Cells[i + 2, 6].Value = p.Status;
        }

        worksheet.Cells.AutoFitColumns();
        await package.SaveAsAsync(new FileInfo(filePath));
        return filePath;
    }

    public static async Task<string> ExportExpensesAsync(List<Expense> expenses)
    {
        var fileName = $"Expenses_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Expenses");

        string[] headers = { "Category", "Amount", "Date", "Description" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        for (int i = 0; i < expenses.Count; i++)
        {
            var e = expenses[i];
            worksheet.Cells[i + 2, 1].Value = e.Category;
            worksheet.Cells[i + 2, 2].Value = e.Amount;
            worksheet.Cells[i + 2, 3].Value = e.Date.ToString("yyyy-MM-dd");
            worksheet.Cells[i + 2, 4].Value = e.Description;
        }

        worksheet.Cells.AutoFitColumns();
        await package.SaveAsAsync(new FileInfo(filePath));
        return filePath;
    }

    public static async Task<string> ExportDailyScheduleAsync(List<Session> sessions, DateTime date, string centerName)
    {
        var fileName = $"DailySchedule_{date:yyyyMMdd}.xlsx";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Daily Schedule");

        var headerBg = System.Drawing.Color.FromArgb(30, 58, 95);
        var headerFg = System.Drawing.Color.White;
        var altBg = System.Drawing.Color.FromArgb(235, 242, 255);

        // Title rows
        ws.Cells[1, 1].Value = centerName;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;
        ws.Cells[2, 1].Value = $"Daily Schedule — {date:dddd, MMMM dd, yyyy}";
        ws.Cells[2, 1].Style.Font.Italic = true;
        ws.Cells[3, 1].Value = $"Total Sessions: {sessions.Count}";
        ws.Cells[3, 1].Style.Font.Italic = true;
        ws.Cells[4, 1].Value = "";

        // Summary sheet
        var summarySheet = package.Workbook.Worksheets.Add("Summary by Therapist");
        summarySheet.Cells[1, 1].Value = "Therapist";
        summarySheet.Cells[1, 2].Value = "Sessions Count";
        summarySheet.Cells[1, 1].Style.Font.Bold = true;
        summarySheet.Cells[1, 2].Style.Font.Bold = true;
        ApplyHeaderStyle(summarySheet.Cells[1, 1, 1, 2], headerBg, headerFg);

        var byTherapist = new Dictionary<string, int>();
        foreach (var s in sessions)
        {
            var tName = s.Therapist?.Name ?? "Unknown";
            byTherapist[tName] = byTherapist.TryGetValue(tName, out var cnt) ? cnt + 1 : 1;
        }
        int sRow = 2;
        foreach (var kvp in byTherapist)
        {
            summarySheet.Cells[sRow, 1].Value = kvp.Key;
            summarySheet.Cells[sRow, 2].Value = kvp.Value;
            sRow++;
        }
        summarySheet.Cells.AutoFitColumns();

        // Headers row
        string[] headers = { "Time", "Beneficiary", "Session Type", "Therapist", "Duration (min)", "Status", "Notes" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[5, i + 1].Value = headers[i];
            ws.Cells[5, i + 1].Style.Font.Bold = true;
        }
        ApplyHeaderStyle(ws.Cells[5, 1, 5, headers.Length], headerBg, headerFg);

        // Data rows
        for (int i = 0; i < sessions.Count; i++)
        {
            var s = sessions[i];
            int row = i + 6;
            ws.Cells[row, 1].Value = s.Time.ToString(@"hh\:mm");
            ws.Cells[row, 2].Value = s.Beneficiary?.Name ?? "";
            ws.Cells[row, 3].Value = s.SessionType;
            ws.Cells[row, 4].Value = s.Therapist?.Name ?? "";
            ws.Cells[row, 5].Value = s.Duration;
            ws.Cells[row, 6].Value = s.Status;
            ws.Cells[row, 7].Value = s.Notes ?? "";

            if (i % 2 == 1)
            {
                for (int c = 1; c <= headers.Length; c++)
                {
                    ws.Cells[row, c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[row, c].Style.Fill.BackgroundColor.SetColor(altBg);
                }
            }
        }

        ws.Cells.AutoFitColumns();
        await package.SaveAsAsync(new FileInfo(filePath));
        return filePath;
    }

    private static void ApplyHeaderStyle(OfficeOpenXml.ExcelRange range, System.Drawing.Color bg, System.Drawing.Color fg)
    {
        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(bg);
        range.Style.Font.Color.SetColor(fg);
        range.Style.Font.Bold = true;
    }
}