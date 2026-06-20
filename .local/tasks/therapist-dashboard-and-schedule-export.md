# Therapist Dashboard + Schedule Export + LAN Access Control

## Objective
Build three interconnected feature sets on top of the existing RehabCenterApp:
1. Daily therapist schedule export (PDF + Excel) and print
2. Full therapist-role dashboard (separate view from Admin) with daily session reports, child worksheets, child history, and export to PDF/Word
3. LAN-only access control for therapist logins (center IP subnet check)

---

## Scope

### A — Daily Schedule Export/Print (Sessions view)
- Add **Export PDF**, **Export Excel**, and **Print** buttons to `SessionsView.axaml`
- Schedule is grouped by therapist, showing all cases for the day with: beneficiary name, session type, time, duration (minutes), status
- `PrintService.ExportDailyScheduleToPdfAsync()` — A4 landscape PDF, branded header (center name/date), table per therapist
- `ExcelExporter.ExportDailyScheduleAsync()` — styled Excel workbook, one sheet per therapist
- `SessionsViewModel` — add `ExportPdfScheduleCommand`, `ExportExcelScheduleCommand`, `PrintScheduleCommand`

### B — New Data Models
**`Models/TherapistReport.cs`**
- `SessionId` (FK), `Session` nav
- `TherapistId` (FK), `Therapist` (Employee) nav  
- `ReportDate`
- `ActivitiesPerformed` (text)
- `ChildResponse` (text)
- `GoalsAddressed` (text) — references IEP goals
- `Homework` (text) — assigned homework for parent
- `BehaviorNotes` (text)
- `OverallRating` (int 1–5)
- `NextSessionPlan` (text)

**`Models/SessionWorksheet.cs`**
- `SessionId` (FK), `Session` nav
- `WorksheetType` (e.g. Motor, Cognitive, Speech, Behavioral)
- `ChecklistItems` (JSON-serialized list of items + completion)
- `MaterialsUsed` (text)
- `ObservationNotes` (text)
- `PhotoPaths` (text — semicolon-delimited file paths)

### C — Database & Context
- Add `DbSet<TherapistReport>` and `DbSet<SessionWorksheet>` to `AppDbContext`
- Seed two new `AppSetting` rows: `"AllowedSubnet"` = `"192.168."` and `"TherapistPortalEnabled"` = `"True"`
- `DatabaseService` new methods:
  - `GetTherapistReportsAsync(int? therapistId, int? beneficiaryId)` — includes Session + Beneficiary
  - `AddTherapistReportAsync(TherapistReport)` / `UpdateTherapistReportAsync`
  - `GetSessionWorksheetsAsync(int sessionId)`
  - `AddSessionWorksheetAsync(SessionWorksheet)` / `UpdateSessionWorksheetAsync`
  - `GetChildHistoryAsync(int beneficiaryId)` — sessions + reports + assessments for a child, ordered by date

### D — Word Export Service
- Install package: `DocumentFormat.OpenXml` (already common in .NET ecosystem)
- **`Services/WordExportService.cs`**:
  - `ExportChildHistoryAsync(Beneficiary, sessions+reports)` → formatted `.docx`
    - Cover page: child name, ID, date range, center logo placeholder
    - Table of contents
    - One section per session: date, therapist, activities, child response, goals, homework
  - `ExportDailyReportAsync(TherapistReport)` → single-session report `.docx`
    - Professional layout with center header, child info, report body sections

### E — LAN Access Service
- **`Services/LanAccessService.cs`**:
  - `GetLocalIpAddresses()` — returns all local IPv4 addresses of the machine
  - `IsAllowedAccess(string allowedSubnet)` — returns `true` if any local IP starts with the subnet string
  - `GetAllowedSubnetAsync()` — reads `AllowedSubnet` from `DatabaseService`
- **Login flow** (`LoginViewModel`):
  - After password verification, if `user.Role == "Therapist"`, call `LanAccessService.IsAllowedAccess()`
  - If fails: show error "Access denied: connect from within the center's network"
  - If passes: route to `TherapistDashboardView` instead of `MainWindow` (Admin view)
- **Admin** login: no IP restriction, always routes to `MainWindow`

### F — Therapist Dashboard View + ViewModel
**`ViewModels/TherapistDashboardViewModel.cs`**
- Properties: `TodaySessions`, `SelectedSession`, `SelectedBeneficiary`, `Reports`, `Worksheets`, `IsReportFormOpen`
- On load: fetch sessions for the logged-in therapist for today
- Commands:
  - `LoadCommand` — load today's sessions + recent reports
  - `OpenReportCommand` — open report form for selected session
  - `ViewChildHistoryCommand` — show history popup for selected beneficiary
  - `ExportHistoryPdfCommand` — export child history as PDF
  - `ExportHistoryWordCommand` — export child history as Word doc
  - `ToggleLanguageCommand` — same bilingual toggle as admin
  - `LogoutCommand` — return to login

**`Views/TherapistDashboardView.axaml`**
- Left panel: today's session list (time, beneficiary name, session type, duration, status) with status badge colors
- Right panel (selected session): session details + "Write Report" / "View Past Reports" / "Add Worksheet" tabs
- Report form (inline modal): all `TherapistReport` fields as labeled text areas + rating stars
- Child history panel: chronological list of sessions+reports; export buttons at top
- Language toggle button (top-right)

**`Views/TherapistWindow.axaml` + `.axaml.cs`**
- Thin wrapper window for therapist role (similar to `MainWindow` but only hosts `TherapistDashboardView`)
- Title: center name + " — Therapist Portal"
- `FlowDirection` bound to `L.FlowDir`

### G — Role Routing in App.axaml.cs + LoginViewModel
- `LoginViewModel` receives two callbacks: `onAdminSuccess` and `onTherapistSuccess`
- `App.axaml.cs` handles both:
  - Admin → show `MainWindow`
  - Therapist → show `TherapistWindow`
- `TherapistWindow` is a standard `Window`; it is modal-independent from `MainWindow`
- Admin can still see all therapist reports inside `MainWindow` via a new "Therapist Reports" sub-section in the Sessions or HR Management area

### H — Settings: LAN Configuration (Admin only)
- `SettingsView.axaml` — new "Network" section with:
  - `AllowedSubnet` TextBox (e.g. "192.168.1." or "10.0.0.")
  - `TherapistPortalEnabled` toggle
  - Explanation label: "Therapist logins are only allowed from IPs matching this prefix"
- `SettingsViewModel` — load/save these two settings

### I — Localization Keys (LocalizationService.cs)
Add keys for all new UI strings in both Arabic and English (approx 30 new keys covering therapist dashboard, report form fields, history view, export buttons, LAN settings).

---

## Files Created (new)
- `Models/TherapistReport.cs`
- `Models/SessionWorksheet.cs`
- `ViewModels/TherapistDashboardViewModel.cs`
- `Views/TherapistDashboardView.axaml` + `.axaml.cs`
- `Views/TherapistWindow.axaml` + `.axaml.cs`
- `Services/WordExportService.cs`
- `Services/LanAccessService.cs`

## Files Modified
- `Services/AppDbContext.cs`
- `Services/DatabaseService.cs`
- `Services/PrintService.cs`
- `Helpers/ExcelExporter.cs`
- `ViewModels/SessionsViewModel.cs`
- `Views/SessionsView.axaml`
- `ViewModels/LoginViewModel.cs`
- `ViewModels/SettingsViewModel.cs`
- `Views/SettingsView.axaml`
- `App.axaml.cs`
- `Services/LocalizationService.cs`

---

## Done When
- [ ] Sessions view has Export PDF / Export Excel / Print buttons that produce a formatted daily schedule grouped by therapist
- [ ] Therapist can log in and sees only their own schedule for the day in a clean dedicated window
- [ ] Therapist login from a non-matching IP is blocked with a clear message
- [ ] Therapist can write a structured daily report for each session and save it
- [ ] Therapist can view the full history of any assigned child (sessions + reports chronologically)
- [ ] Child history exports to a formatted PDF and a formatted Word (.docx) file
- [ ] Admin can configure the allowed LAN subnet in Settings
- [ ] Build passes with zero errors
