---
name: Role-based user system
description: How roles, nav visibility, LAN/WiFi check, and UserSessionService are wired together in this Avalonia app
---

## Roles defined
Admin, Therapist, Receptionist, Accountant, HR, PR (علاقات عامة)

## Key files
- `Services/UserSessionService.cs` — singleton; holds logged-in user, exposes CanSeeXxx booleans per section
- `Models/User.cs` — has FullName, Phone, Email, RequireLanAccess, Notes (added)
- `ViewModels/LoginViewModel.cs` — LAN check applies to ALL non-Admin roles (not just Therapist); routes: Admin→MainWindow, Therapist→TherapistWindow, others→MainWindow with role-filtered nav
- `ViewModels/MainWindowViewModel.cs` — exposes CanSeeXxx properties bound from UserSessionService; has NavigateToUserManagementCommand
- `Views/MainWindow.axaml` — uses x:CompileBindings="False"; every nav button has IsVisible="{Binding CanSeeXxx}"
- `ViewModels/UserManagementViewModel.cs` — CRUD for users + LAN subnet config (auto-detect + manual)
- `Views/UserManagementView.axaml` — user list, add/edit form overlay, WiFi subnet settings panel
- `Services/NavigationService.cs` — includes UserManagementViewModel
- `App.axaml.cs` — routes login result to correct window; registers UserManagementViewModel as Singleton

## Role → sections mapping
Admin: everything + UserManagement + Settings
Therapist: TherapistWindow (separate, own sessions only)
Receptionist: Dashboard, Beneficiaries, WaitingList, Sessions, Telehealth, Correspondence, ParentPortal, Reminders, Forms
Accountant: Dashboard, Accounting, Inventory, HR (view), Analytics, GovernmentReports
HR: Dashboard, HRManagement
PR: Dashboard, Beneficiaries, Correspondence, ParentPortal, Reminders, Documents, Forms

## LAN/WiFi check
- AllowedSubnet stored in AppSettings DB key (default "192.168.")
- LanAccessService.IsAllowedAccess() checks local IPs start with that prefix
- Applied to ALL non-Admin roles on login if user.RequireLanAccess == true
- Admin can set subnet manually or auto-detect from own IP in UserManagement page

## Why
- MainWindow uses x:CompileBindings="False" to avoid AVLN3000 errors with MaterialIcon string Kind bindings
- UserSessionService is a singleton (not DI-registered) — accessed via Instance property — avoids circular DI
