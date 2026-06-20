using System;
using System.Collections.Generic;
using Avalonia.Media;
using ReactiveUI;

namespace RehabCenterApp.Services;

public class LocalizationService : ReactiveObject
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    public static event EventHandler? LanguageChanged;

    private bool _isArabic = true;
    public bool IsArabic
    {
        get => _isArabic;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isArabic, value);
            this.RaisePropertyChanged(nameof(FlowDir));
            this.RaisePropertyChanged(nameof(LangToggleLabel));
            this.RaisePropertyChanged("Item[]");
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public FlowDirection FlowDir => _isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    public string LangToggleLabel => _isArabic ? "EN" : "ع";

    public string this[string key]
    {
        get
        {
            var dict = _isArabic ? _arabic : _english;
            return dict.TryGetValue(key, out var val) ? val : key;
        }
    }

    public void ToggleLanguage() => IsArabic = !IsArabic;

    private static readonly Dictionary<string, string> _arabic = new()
    {
        // Common
        ["Save"] = "حفظ",
        ["Cancel"] = "إلغاء",
        ["Add"] = "إضافة",
        ["Edit"] = "تعديل",
        ["Delete"] = "حذف",
        ["Search"] = "بحث سريع...",
        ["Name"] = "الاسم",
        ["Date"] = "التاريخ",
        ["Status"] = "الحالة",
        ["Notes"] = "ملاحظات",
        ["Amount"] = "المبلغ",
        ["Type"] = "النوع",
        ["Description"] = "الوصف",
        ["Export"] = "تصدير",
        ["Print"] = "طباعة",
        ["Yes"] = "نعم",
        ["No"] = "لا",
        ["New"] = "جديد...",
        ["Number"] = "الرقم",
        ["Excel"] = "Excel",

        // Login
        ["LoginTitle"] = "تسجيل الدخول - مركز التأهيل",
        ["AppName"] = "مركز التأهيل",
        ["AppSubtitle"] = "نظام الإدارة المتكامل",
        ["Username"] = "اسم المستخدم",
        ["UsernamePlaceholder"] = "أدخل اسم المستخدم",
        ["Password"] = "كلمة المرور",
        ["PasswordPlaceholder"] = "أدخل كلمة المرور",
        ["LoginBtn"] = "تسجيل الدخول",
        ["ForgotPassword"] = "نسيت كلمة المرور؟",

        // Navigation
        ["AppTitle"] = "مركز التأهيل المتكامل - نظام الإدارة المتقدم",
        ["NavCoreOps"] = "العمليات الأساسية",
        ["NavDashboard"] = "لوحة التحكم",
        ["NavBeneficiaries"] = "المستفيدين",
        ["NavWaitingList"] = "قائمة الانتظار",
        ["NavSessions"] = "الجلسات",
        ["NavTelehealth"] = "جلسات عن بعد",
        ["NavClinical"] = "السريري والعلاجي",
        ["NavAssessments"] = "التقييمات المعيارية",
        ["NavInterventionPlans"] = "خطط التدخل (IEP)",
        ["NavClinicalReports"] = "التقارير السريرية",
        ["NavMDT"] = "اجتماعات الفريق",
        ["NavGamification"] = "نظام التحفيز",
        ["NavFinancial"] = "المالية والإدارية",
        ["NavAccounting"] = "المحاسبة",
        ["NavInventory"] = "المخزون",
        ["NavHR"] = "الموارد البشرية",
        ["NavCommunication"] = "التواصل والمستندات",
        ["NavCorrespondence"] = "الصادر والوارد",
        ["NavParentPortal"] = "تواصل أولياء الأمور",
        ["NavReminders"] = "التذكيرات",
        ["NavDocuments"] = "أرشفة المستندات",
        ["NavReports"] = "التقارير والتحليلات",
        ["NavAnalytics"] = "التحليلات المتقدمة",
        ["NavGovernmentReports"] = "التقارير الحكومية",
        ["NavForms"] = "الاستمارات",
        ["NavSettings"] = "الإعدادات",
        ["NavLogout"] = "تسجيل الخروج",

        // Dashboard
        ["Dashboard"] = "لوحة التحكم",
        ["Beneficiaries"] = "المستفيدين",
        ["TodaySessions"] = "جلسات اليوم",
        ["MonthRevenue"] = "إيرادات الشهر",
        ["MonthExpenses"] = "مصروفات الشهر",
        ["RevenueVsExpenses"] = "الإيرادات vs المصروفات",
        ["MonthlyBeneficiaries"] = "المستفيدين شهرياً",
        ["DisabilityDistribution"] = "توزيع الإعاقات",
        ["UpcomingReminders"] = "تذكيرات قادمة",

        // Beneficiaries
        ["FullName"] = "الاسم الكامل *",
        ["DateOfBirth"] = "تاريخ الميلاد *",
        ["Gender"] = "الجنس",
        ["Male"] = "ذكر",
        ["Female"] = "أنثى",
        ["NationalId"] = "رقم الهوية",
        ["Address"] = "العنوان",
        ["Phone"] = "رقم الجوال",
        ["DisabilityType"] = "نوع الإعاقة *",
        ["Diagnosis"] = "التشخيص",
        ["GuardianName"] = "اسم ولي الأمر",
        ["GuardianPhone"] = "جوال ولي الأمر",
        ["InsuranceCompany"] = "شركة التأمين",
        ["InsuranceNumber"] = "رقم التأمين",
        ["Age"] = "العمر",
        ["LastSession"] = "آخر جلسة",
        ["AddBeneficiary"] = "إضافة مستفيد جديد",
        ["EditBeneficiary"] = "تعديل مستفيد",
        ["SearchPlaceholder"] = "بحث سريع...",

        // Sessions
        ["NewAppointment"] = "موعد جديد",
        ["Present"] = "حضور",
        ["Absent"] = "غياب",
        ["Cancelled"] = "إلغاء",
        ["Time"] = "الوقت",
        ["Beneficiary"] = "المستفيد",
        ["SessionType"] = "نوع الجلسة",
        ["Therapist"] = "المعالج",
        ["Duration"] = "المدة",
        ["AddSession"] = "إضافة موعد جديد",
        ["BeneficiaryReq"] = "المستفيد *",
        ["TherapistReq"] = "المعالج *",
        ["SessionTypeReq"] = "نوع الجلسة *",
        ["DurationMinutes"] = "المدة (دقيقة)",

        // Accounting
        ["TotalRevenue"] = "إجمالي الإيرادات",
        ["TotalExpenses"] = "إجمالي المصروفات",
        ["NetProfit"] = "صافي الربح",
        ["Period"] = "الفترة",
        ["To"] = "إلى",
        ["Revenue"] = "الإيرادات",
        ["AddPayment"] = "إضافة دفعة",
        ["PrintReceipt"] = "طباعة إيصال",
        ["ReceiptNumber"] = "رقم الإيصال",
        ["PaymentType"] = "نوع الدفع",
        ["Expenses"] = "المصروفات",
        ["AddExpense"] = "إضافة مصروف",
        ["Category"] = "التصنيف",
        ["AddNewPayment"] = "إضافة دفعة جديدة",
        ["AddNewExpense"] = "إضافة مصروف جديد",
        ["CategoryReq"] = "التصنيف *",
        ["AmountReq"] = "المبلغ *",
        ["AttachInvoice"] = "إرفاق فاتورة",
        ["ChooseFile"] = "اختيار ملف",

        // Settings
        ["Settings"] = "الإعدادات",
        ["CenterInfo"] = "بيانات المركز",
        ["CenterName"] = "اسم المركز",
        ["CenterAddress"] = "العنوان",
        ["CenterPhone"] = "الجوال",
        ["Email"] = "البريد الإلكتروني",
        ["UsersAndRoles"] = "المستخدمين والصلاحيات",
        ["Role"] = "الدور",
        ["CustomCategories"] = "التصنيفات المخصصة",
        ["DisabilityTypes"] = "أنواع الإعاقة",
        ["SessionTypes"] = "أنواع الجلسات",
        ["ExpenseCategories"] = "تصنيفات المصروفات",
        ["NewPlaceholder"] = "جديد...",
        ["Backup"] = "النسخ الاحتياطي",
        ["CreateBackup"] = "إنشاء نسخة احتياطية",
        ["Language"] = "اللغة",

        // HR
        ["HRManagement"] = "إدارة الموارد البشرية",
        ["AddEmployee"] = "إضافة موظف",
        ["RequestLeave"] = "طلب إجازة",
        ["Evaluate"] = "تقييم",
        ["Payroll"] = "كشف الرواتب",
        ["Employees"] = "الموظفون",
        ["LeaveRequests"] = "طلبات الإجازة",
        ["SubmitLeave"] = "رفع الطلب",
        ["LeaveType"] = "نوع الإجازة",
        ["StartDate"] = "تاريخ البداية",
        ["EndDate"] = "تاريخ النهاية",
        ["Reason"] = "السبب",
        ["Employee"] = "الموظف",
        ["Days"] = "الأيام",
        ["Salary"] = "الراتب",
        ["JoinDate"] = "تاريخ التوظيف",
        ["Active"] = "نشط",

        // Inventory
        ["InventoryManagement"] = "إدارة المخزون",
        ["AddItem"] = "إضافة صنف",
        ["AddStock"] = "+1 مخزون",
        ["RemoveStock"] = "-1 مخزون",
        ["Quantity"] = "الكمية",
        ["MinStock"] = "الحد الأدنى",
        ["UnitCost"] = "تكلفة الوحدة",
        ["AddInventoryItem"] = "إضافة صنف مخزون",
        ["NameReq"] = "الاسم *",

        // Assessments
        ["StandardAssessments"] = "التقييمات المعيارية",
        ["NewAssessment"] = "تقييم جديد",
        ["TrackProgress"] = "تتبع التقدم",
        ["ProgressChart"] = "مخطط التقدم",
        ["RawScore"] = "الدرجة الخام",
        ["StandardScore"] = "الدرجة المعيارية",
        ["SeverityLevel"] = "مستوى الشدة",
        ["Recommendations"] = "التوصيات",
        ["Assessment"] = "التقييم",
        ["AssessmentReq"] = "التقييم *",

        // Clinical Reports
        ["ClinicalReports"] = "التقارير السريرية",
        ["NewReport"] = "تقرير جديد",
        ["Approve"] = "اعتماد",
        ["Sign"] = "توقيع رقمي",
        ["ReportType"] = "نوع التقرير",
        ["PreparedBy"] = "أعده",
        ["NewClinicalReport"] = "تقرير سريري جديد",
        ["ContentReq"] = "المحتوى *",

        // Analytics
        ["AdvancedAnalytics"] = "التحليلات المتقدمة والذكاء الاصطناعي",
        ["TotalBeneficiaries"] = "إجمالي المستفيدين",
        ["MonthlyRevenue"] = "الإيرادات الشهرية",
        ["SessionCompletion"] = "اكتمال الجلسات",
        ["ParentSatisfaction"] = "رضا الوالدين",
        ["AtRisk"] = "في خطر",
        ["RevenueTrend"] = "اتجاه الإيرادات",
        ["AIPredictions"] = "توقعات الذكاء الاصطناعي",
        ["GeneratePredictions"] = "توليد التوقعات",
        ["Domain"] = "المجال",
        ["Current"] = "الحالي",
        ["ThreeMonths"] = "3 أشهر",
        ["SixMonths"] = "6 أشهر",
        ["TwelveMonths"] = "12 شهر",
        ["Risk"] = "الخطر",

        // Reminders
        ["Reminders"] = "التذكيرات",
        ["NewReminder"] = "تذكير جديد",
        ["Complete"] = "إتمام",
        ["Snooze"] = "تأجيل",
        ["Overdue"] = "متأخرة",
        ["Today"] = "اليوم",
        ["Upcoming"] = "قادمة",
        ["Title"] = "العنوان",
        ["TitleReq"] = "العنوان *",
        ["DateReq"] = "التاريخ *",
        ["TimeReq"] = "الوقت *",
        ["Priority"] = "الأولوية",
        ["LinkBeneficiary"] = "ربط بمستفيد (اختياري)",

        // Correspondence
        ["Correspondence"] = "الصادر والوارد",
        ["NewTransaction"] = "معاملة جديدة",
        ["SearchPlaceholder2"] = "بحث...",
        ["Entity"] = "الجهة",
        ["Subject"] = "الموضوع",

        // Forms
        ["FormsAndPrinting"] = "الاستمارات والطباعة",
        ["FormTemplates"] = "قوالب الاستمارات",
        ["BeneficiaryRegistration"] = "استمارة تسجيل مستفيد",
        ["InitialAssessment"] = "استمارة تقييم أولي",
        ["IEPPlan"] = "خطة علاجية (IEP)",
        ["FollowUpReport"] = "تقرير متابعة",
        ["GuardianConsent"] = "موافقة ولي الأمر",
        ["PrintPreview"] = "معاينة الطباعة",
        ["ExportPDF"] = "تصدير PDF",

        // Waiting List
        ["WaitingList"] = "قائمة الانتظار",
        ["AddEntry"] = "إضافة",
        ["Convert"] = "تحويل لمستفيد",
        ["Contact"] = "تواصل",
        ["NewWaitingEntry"] = "طلب انتظار جديد",
        ["Registered"] = "تاريخ التسجيل",

        // MDT Meetings
        ["MDTMeetings"] = "اجتماعات الفريق",
        ["NewMeeting"] = "اجتماع جديد",
        ["ViewMinutes"] = "محضر الاجتماع",
        ["Attendees"] = "المشاركون",
        ["MeetingDate"] = "تاريخ الاجتماع",
        ["Agenda"] = "جدول الأعمال",

        // Telehealth
        ["Telehealth"] = "جلسات عن بعد",
        ["ScheduleSession"] = "جدولة جلسة",
        ["StartSession"] = "بدء الجلسة",
        ["Link"] = "الرابط",
        ["Platform"] = "المنصة",

        // Gamification
        ["Gamification"] = "نظام التحفيز",
        ["AddAchievement"] = "إضافة إنجاز",
        ["AwardPoints"] = "منح نقاط",
        ["Achievements"] = "الإنجازات",
        ["Points"] = "النقاط",
        ["Level"] = "المستوى",
        ["Badge"] = "الشارة",

        // Government Reports
        ["GovernmentReports"] = "التقارير الحكومية",
        ["GenerateReport"] = "توليد تقرير",
        ["SubmitReport"] = "رفع التقرير",
        ["Quarter"] = "الربع",
        ["Year"] = "السنة",

        // Document Archive
        ["DocumentArchive"] = "أرشيف المستندات",
        ["UploadDocument"] = "رفع مستند",
        ["DownloadDocument"] = "تنزيل",
        ["DocumentType"] = "نوع المستند",
        ["UploadedBy"] = "رُفع بواسطة",

        // Intervention Plans
        ["InterventionPlans"] = "خطط التدخل (IEP)",
        ["NewPlan"] = "خطة جديدة",
        ["ViewPlan"] = "عرض الخطة",
        ["Goals"] = "الأهداف",

        // Parent Portal
        ["ParentPortal"] = "تواصل أولياء الأمور",
        ["SendMessage"] = "إرسال رسالة",
        ["SurveyResults"] = "نتائج الاستبيان",
        ["Message"] = "الرسالة",
        ["Survey"] = "الاستبيان",
        ["Rating"] = "التقييم",

        // Therapist Portal
        ["TherapistPortal"] = "بوابة المعالج",
        ["TodaySchedule"] = "جدول اليوم",
        ["TodayAppointments"] = "مواعيد اليوم",
        ["TotalReports"] = "إجمالي التقارير",
        ["WriteReport"] = "كتابة تقرير",
        ["SaveReport"] = "حفظ التقرير",
        ["DailySessionReport"] = "تقرير الجلسة اليومي",
        ["ReportExistsFor"] = "تم حفظ تقرير لهذه الجلسة",
        ["RecentReports"] = "آخر التقارير",
        ["Activities"] = "الأنشطة",
        ["ActivitiesPerformed"] = "الأنشطة المنفذة",
        ["ChildResponse"] = "استجابة الطفل",
        ["GoalsAddressed"] = "الأهداف المُناولة",
        ["BehaviorNotes"] = "ملاحظات السلوك",
        ["HomeworkParent"] = "الواجبات / تعليمات ولي الأمر",
        ["NextSessionPlan"] = "خطة الجلسة القادمة",
        ["OverallRating"] = "التقييم العام",
        ["Worksheet"] = "ورقة عمل الجلسة",
        ["WorksheetDescription"] = "ستظهر أوراق العمل هنا. اختر جلسة من اليسار للبدء.",
        ["SelectSessionFirst"] = "الرجاء اختيار جلسة من القائمة.",
        ["SelectSessionPrompt"] = "انقر على جلسة لعرض التفاصيل وكتابة تقرير.",
        ["ChildHistory"] = "تاريخ تقدم الطفل",
        ["ExportWord"] = "تصدير Word",
        ["HistoryNote"] = "صدّر التاريخ الكامل كـ PDF أو Word لمشاركته مع الوالدين أو المشرفين.",
        ["NavLogout"] = "تسجيل الخروج",

        // Schedule Export
        ["ExportSchedule"] = "تصدير الجدول",
        ["PrintSchedule"] = "طباعة الجدول",
        ["ExportExcel"] = "تصدير Excel",
        ["ScheduleExported"] = "تم تصدير الجدول",

        // Settings LAN
        ["LanAccessControl"] = "التحكم في الوصول عبر الشبكة المحلية",
        ["AllowedSubnet"] = "الشبكة الفرعية المسموح بها",
        ["AllowedSubnetHint"] = "مثال: 192.168. (فقط الأجهزة في هذه الشبكة يمكنها تسجيل دخول المعالج)",
        ["TherapistPortalEnabled"] = "تفعيل بوابة المعالج",
        ["CurrentIp"] = "عنوان IP الحالي",
        ["LanSettingsSaved"] = "تم حفظ إعدادات الشبكة بنجاح.",
    };

    private static readonly Dictionary<string, string> _english = new()
    {
        // Common
        ["Save"] = "Save",
        ["Cancel"] = "Cancel",
        ["Add"] = "Add",
        ["Edit"] = "Edit",
        ["Delete"] = "Delete",
        ["Search"] = "Quick Search...",
        ["Name"] = "Name",
        ["Date"] = "Date",
        ["Status"] = "Status",
        ["Notes"] = "Notes",
        ["Amount"] = "Amount",
        ["Type"] = "Type",
        ["Description"] = "Description",
        ["Export"] = "Export",
        ["Print"] = "Print",
        ["Yes"] = "Yes",
        ["No"] = "No",
        ["New"] = "New...",
        ["Number"] = "Number",
        ["Excel"] = "Excel",

        // Login
        ["LoginTitle"] = "Login - Rehab Center",
        ["AppName"] = "Rehab Center",
        ["AppSubtitle"] = "Integrated Management System",
        ["Username"] = "Username",
        ["UsernamePlaceholder"] = "Enter username",
        ["Password"] = "Password",
        ["PasswordPlaceholder"] = "Enter password",
        ["LoginBtn"] = "Login",
        ["ForgotPassword"] = "Forgot Password?",

        // Navigation
        ["AppTitle"] = "Integrated Rehab Center - Advanced Management",
        ["NavCoreOps"] = "Core Operations",
        ["NavDashboard"] = "Dashboard",
        ["NavBeneficiaries"] = "Beneficiaries",
        ["NavWaitingList"] = "Waiting List",
        ["NavSessions"] = "Sessions",
        ["NavTelehealth"] = "Telehealth Sessions",
        ["NavClinical"] = "Clinical & Therapeutic",
        ["NavAssessments"] = "Standard Assessments",
        ["NavInterventionPlans"] = "Intervention Plans (IEP)",
        ["NavClinicalReports"] = "Clinical Reports",
        ["NavMDT"] = "Team Meetings",
        ["NavGamification"] = "Motivation System",
        ["NavFinancial"] = "Financial & Administrative",
        ["NavAccounting"] = "Accounting",
        ["NavInventory"] = "Inventory",
        ["NavHR"] = "Human Resources",
        ["NavCommunication"] = "Communication & Documents",
        ["NavCorrespondence"] = "Correspondence",
        ["NavParentPortal"] = "Parent Communication",
        ["NavReminders"] = "Reminders",
        ["NavDocuments"] = "Document Archive",
        ["NavReports"] = "Reports & Analytics",
        ["NavAnalytics"] = "Advanced Analytics",
        ["NavGovernmentReports"] = "Government Reports",
        ["NavForms"] = "Forms",
        ["NavSettings"] = "Settings",
        ["NavLogout"] = "Logout",

        // Dashboard
        ["Dashboard"] = "Dashboard",
        ["Beneficiaries"] = "Beneficiaries",
        ["TodaySessions"] = "Today's Sessions",
        ["MonthRevenue"] = "Monthly Revenue",
        ["MonthExpenses"] = "Monthly Expenses",
        ["RevenueVsExpenses"] = "Revenue vs Expenses",
        ["MonthlyBeneficiaries"] = "Monthly Beneficiaries",
        ["DisabilityDistribution"] = "Disability Distribution",
        ["UpcomingReminders"] = "Upcoming Reminders",

        // Beneficiaries
        ["FullName"] = "Full Name *",
        ["DateOfBirth"] = "Date of Birth *",
        ["Gender"] = "Gender",
        ["Male"] = "Male",
        ["Female"] = "Female",
        ["NationalId"] = "National ID",
        ["Address"] = "Address",
        ["Phone"] = "Phone",
        ["DisabilityType"] = "Disability Type *",
        ["Diagnosis"] = "Diagnosis",
        ["GuardianName"] = "Guardian Name",
        ["GuardianPhone"] = "Guardian Phone",
        ["InsuranceCompany"] = "Insurance Company",
        ["InsuranceNumber"] = "Insurance Number",
        ["Age"] = "Age",
        ["LastSession"] = "Last Session",
        ["AddBeneficiary"] = "Add New Beneficiary",
        ["EditBeneficiary"] = "Edit Beneficiary",
        ["SearchPlaceholder"] = "Quick Search...",

        // Sessions
        ["NewAppointment"] = "New Appointment",
        ["Present"] = "Present",
        ["Absent"] = "Absent",
        ["Cancelled"] = "Cancel",
        ["Time"] = "Time",
        ["Beneficiary"] = "Beneficiary",
        ["SessionType"] = "Session Type",
        ["Therapist"] = "Therapist",
        ["Duration"] = "Duration",
        ["AddSession"] = "Add New Appointment",
        ["BeneficiaryReq"] = "Beneficiary *",
        ["TherapistReq"] = "Therapist *",
        ["SessionTypeReq"] = "Session Type *",
        ["DurationMinutes"] = "Duration (minutes)",

        // Accounting
        ["TotalRevenue"] = "Total Revenue",
        ["TotalExpenses"] = "Total Expenses",
        ["NetProfit"] = "Net Profit",
        ["Period"] = "Period",
        ["To"] = "To",
        ["Revenue"] = "Revenue",
        ["AddPayment"] = "Add Payment",
        ["PrintReceipt"] = "Print Receipt",
        ["ReceiptNumber"] = "Receipt No.",
        ["PaymentType"] = "Payment Type",
        ["Expenses"] = "Expenses",
        ["AddExpense"] = "Add Expense",
        ["Category"] = "Category",
        ["AddNewPayment"] = "Add New Payment",
        ["AddNewExpense"] = "Add New Expense",
        ["CategoryReq"] = "Category *",
        ["AmountReq"] = "Amount *",
        ["AttachInvoice"] = "Attach Invoice",
        ["ChooseFile"] = "Choose File",

        // Settings
        ["Settings"] = "Settings",
        ["CenterInfo"] = "Center Information",
        ["CenterName"] = "Center Name",
        ["CenterAddress"] = "Address",
        ["CenterPhone"] = "Phone",
        ["Email"] = "Email",
        ["UsersAndRoles"] = "Users & Roles",
        ["Role"] = "Role",
        ["CustomCategories"] = "Custom Categories",
        ["DisabilityTypes"] = "Disability Types",
        ["SessionTypes"] = "Session Types",
        ["ExpenseCategories"] = "Expense Categories",
        ["NewPlaceholder"] = "New...",
        ["Backup"] = "Backup",
        ["CreateBackup"] = "Create Backup",
        ["Language"] = "Language",

        // HR
        ["HRManagement"] = "HR Management",
        ["AddEmployee"] = "Add Employee",
        ["RequestLeave"] = "Request Leave",
        ["Evaluate"] = "Evaluate",
        ["Payroll"] = "Payroll",
        ["Employees"] = "Employees",
        ["LeaveRequests"] = "Leave Requests",
        ["SubmitLeave"] = "Submit",
        ["LeaveType"] = "Leave Type",
        ["StartDate"] = "Start Date",
        ["EndDate"] = "End Date",
        ["Reason"] = "Reason",
        ["Employee"] = "Employee",
        ["Days"] = "Days",
        ["Salary"] = "Salary",
        ["JoinDate"] = "Join Date",
        ["Active"] = "Active",

        // Inventory
        ["InventoryManagement"] = "Inventory Management",
        ["AddItem"] = "Add Item",
        ["AddStock"] = "+1 Stock",
        ["RemoveStock"] = "-1 Stock",
        ["Quantity"] = "Quantity",
        ["MinStock"] = "Min Stock",
        ["UnitCost"] = "Unit Cost",
        ["AddInventoryItem"] = "Add Inventory Item",
        ["NameReq"] = "Name *",

        // Assessments
        ["StandardAssessments"] = "Standardized Assessments",
        ["NewAssessment"] = "New Assessment",
        ["TrackProgress"] = "Track Progress",
        ["ProgressChart"] = "Progress Chart",
        ["RawScore"] = "Raw Score",
        ["StandardScore"] = "Standard Score",
        ["SeverityLevel"] = "Severity Level",
        ["Recommendations"] = "Recommendations",
        ["Assessment"] = "Assessment",
        ["AssessmentReq"] = "Assessment *",

        // Clinical Reports
        ["ClinicalReports"] = "Clinical Reports",
        ["NewReport"] = "New Report",
        ["Approve"] = "Approve",
        ["Sign"] = "Sign",
        ["ReportType"] = "Report Type",
        ["PreparedBy"] = "Prepared By",
        ["NewClinicalReport"] = "New Clinical Report",
        ["ContentReq"] = "Content *",

        // Analytics
        ["AdvancedAnalytics"] = "Advanced Analytics & AI Predictions",
        ["TotalBeneficiaries"] = "Total Beneficiaries",
        ["MonthlyRevenue"] = "Monthly Revenue",
        ["SessionCompletion"] = "Session Completion",
        ["ParentSatisfaction"] = "Parent Satisfaction",
        ["AtRisk"] = "At Risk",
        ["RevenueTrend"] = "Revenue Trend",
        ["AIPredictions"] = "AI Progress Predictions",
        ["GeneratePredictions"] = "Generate Predictions",
        ["Domain"] = "Domain",
        ["Current"] = "Current",
        ["ThreeMonths"] = "3 Months",
        ["SixMonths"] = "6 Months",
        ["TwelveMonths"] = "12 Months",
        ["Risk"] = "Risk",

        // Reminders
        ["Reminders"] = "Reminders",
        ["NewReminder"] = "New Reminder",
        ["Complete"] = "Complete",
        ["Snooze"] = "Snooze",
        ["Overdue"] = "Overdue",
        ["Today"] = "Today",
        ["Upcoming"] = "Upcoming",
        ["Title"] = "Title",
        ["TitleReq"] = "Title *",
        ["DateReq"] = "Date *",
        ["TimeReq"] = "Time *",
        ["Priority"] = "Priority",
        ["LinkBeneficiary"] = "Link to Beneficiary (optional)",

        // Correspondence
        ["Correspondence"] = "Correspondence",
        ["NewTransaction"] = "New Transaction",
        ["SearchPlaceholder2"] = "Search...",
        ["Entity"] = "Entity",
        ["Subject"] = "Subject",

        // Forms
        ["FormsAndPrinting"] = "Forms & Printing",
        ["FormTemplates"] = "Form Templates",
        ["BeneficiaryRegistration"] = "Beneficiary Registration Form",
        ["InitialAssessment"] = "Initial Assessment Form",
        ["IEPPlan"] = "Therapeutic Plan (IEP)",
        ["FollowUpReport"] = "Follow-up Report",
        ["GuardianConsent"] = "Guardian Consent",
        ["PrintPreview"] = "Print Preview",
        ["ExportPDF"] = "Export PDF",

        // Waiting List
        ["WaitingList"] = "Waiting List",
        ["AddEntry"] = "Add Entry",
        ["Convert"] = "Convert",
        ["Contact"] = "Contact",
        ["NewWaitingEntry"] = "New Waiting List Entry",
        ["Registered"] = "Registered",

        // MDT Meetings
        ["MDTMeetings"] = "MDT Team Meetings",
        ["NewMeeting"] = "New Meeting",
        ["ViewMinutes"] = "View Minutes",
        ["Attendees"] = "Attendees",
        ["MeetingDate"] = "Meeting Date",
        ["Agenda"] = "Agenda",

        // Telehealth
        ["Telehealth"] = "Telehealth Sessions",
        ["ScheduleSession"] = "Schedule Session",
        ["StartSession"] = "Start Session",
        ["Link"] = "Link",
        ["Platform"] = "Platform",

        // Gamification
        ["Gamification"] = "Motivation System",
        ["AddAchievement"] = "Add Achievement",
        ["AwardPoints"] = "Award Points",
        ["Achievements"] = "Achievements",
        ["Points"] = "Points",
        ["Level"] = "Level",
        ["Badge"] = "Badge",

        // Government Reports
        ["GovernmentReports"] = "Government Reports",
        ["GenerateReport"] = "Generate Report",
        ["SubmitReport"] = "Submit Report",
        ["Quarter"] = "Quarter",
        ["Year"] = "Year",

        // Document Archive
        ["DocumentArchive"] = "Document Archive",
        ["UploadDocument"] = "Upload Document",
        ["DownloadDocument"] = "Download",
        ["DocumentType"] = "Document Type",
        ["UploadedBy"] = "Uploaded By",

        // Intervention Plans
        ["InterventionPlans"] = "Intervention Plans (IEP)",
        ["NewPlan"] = "New Plan",
        ["ViewPlan"] = "View Plan",
        ["Goals"] = "Goals",

        // Parent Portal
        ["ParentPortal"] = "Parent Communication Portal",
        ["SendMessage"] = "Send Message",
        ["SurveyResults"] = "Survey Results",
        ["Message"] = "Message",
        ["Survey"] = "Survey",
        ["Rating"] = "Rating",

        // Therapist Portal
        ["TherapistPortal"] = "Therapist Portal",
        ["TodaySchedule"] = "Today's Schedule",
        ["TodayAppointments"] = "Today's Appointments",
        ["TotalReports"] = "Total Reports",
        ["WriteReport"] = "Write Report",
        ["SaveReport"] = "Save Report",
        ["DailySessionReport"] = "Daily Session Report",
        ["ReportExistsFor"] = "Report saved for this session",
        ["RecentReports"] = "Recent Reports",
        ["Activities"] = "Activities",
        ["ActivitiesPerformed"] = "Activities Performed",
        ["ChildResponse"] = "Child Response",
        ["GoalsAddressed"] = "Goals Addressed",
        ["BehaviorNotes"] = "Behavior Notes",
        ["HomeworkParent"] = "Homework / Parent Instructions",
        ["NextSessionPlan"] = "Next Session Plan",
        ["OverallRating"] = "Overall Rating",
        ["Worksheet"] = "Session Worksheet",
        ["WorksheetDescription"] = "Session worksheets will appear here. Select a session from the left to get started.",
        ["SelectSessionFirst"] = "Please select a session from the list on the left.",
        ["SelectSessionPrompt"] = "Click on a session to view details and write a report.",
        ["ChildHistory"] = "Child Progress History",
        ["ExportWord"] = "Export Word",
        ["HistoryNote"] = "Export the full history as PDF or Word to share with parents or supervisors.",
        ["NavLogout"] = "Logout",

        // Schedule Export
        ["ExportSchedule"] = "Export Schedule",
        ["PrintSchedule"] = "Print Schedule",
        ["ExportExcel"] = "Export Excel",
        ["ScheduleExported"] = "Schedule exported",

        // Settings LAN
        ["LanAccessControl"] = "LAN Access Control",
        ["AllowedSubnet"] = "Allowed IP Subnet",
        ["AllowedSubnetHint"] = "e.g. 192.168. (only devices on this subnet can log in as Therapist)",
        ["TherapistPortalEnabled"] = "Therapist Portal Enabled",
        ["CurrentIp"] = "Your Current IP",
        ["LanSettingsSaved"] = "LAN settings saved successfully.",
    };

}
