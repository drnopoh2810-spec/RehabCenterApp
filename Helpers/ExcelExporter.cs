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
}