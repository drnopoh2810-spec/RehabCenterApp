namespace RehabCenterApp.Helpers;

public static class Constants
{
    public const string AppName = "مركز التأهيل";
    public const string AppVersion = "1.0.0";
    public const string DefaultCurrency = "ر.س";

    public static readonly string[] DisabilityTypes = 
    {
        "إعاقة حركية",
        "إعاقة ذهنية",
        "إعاقة سمعية",
        "إعاقة بصرية",
        "إعاقة نطقية",
        "توحد",
        "متلازمة داون",
        "أخرى"
    };

    public static readonly string[] SessionTypes =
    {
        "علاج طبيعي",
        "علاج وظيفي",
        "علاج نطق",
        "علاج نفسي",
        "تربية خاصة",
        "تكامل حسي",
        "تنمية مهارات"
    };

    public static readonly string[] ExpenseCategories =
    {
        "رواتب",
        "إيجار",
        "معدات طبية",
        "أدوات علاجية",
        "كهرباء",
        "ماء",
        "صيانة",
        "تسويق",
        "أخرى"
    };

    public static readonly string[] PaymentTypes =
    {
        "نقدي",
        "تحويل بنكي",
        "بطاقة",
        "تأمين"
    };
}