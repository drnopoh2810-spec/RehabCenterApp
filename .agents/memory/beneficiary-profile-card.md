---
name: Beneficiary profile card
description: Architecture of the 5-tab comprehensive beneficiary profile overlay
---

## Overview
Full-screen overlay shown inside `BeneficiariesView.axaml` via `IsProfileOpen` + `ProfileViewModel`.

## Key files
- `ViewModels/BeneficiaryProfileViewModel.cs` — data, tabs, history composition
- `Views/BeneficiaryProfileView.axaml` — 5-tab AXAML (x:CompileBindings="False")
- `Models/BeneficiaryAttachment.cs` — new model for file attachments
- `Helpers/TopLevelHelper.cs` — `GetTopLevel()` for StorageProvider file picker

## Tabs
1. البيانات الأساسية — editable personal + guardian info, SaveBasicInfoCommand
2. التشخيص الطبي — diagnosis, medical history, blood type, medications, SaveMedicalCommand
3. المرفقات — BeneficiaryAttachment list, AddAttachmentCommand (file picker), OpenAttachment, Delete
4. الخطط التأهلية — InterventionPlan list with objectives
5. السجل التاريخي — HistoryEntry timeline composed from: Sessions, TherapistReports, Assessments, ObjectiveProgress

## History composition (BeneficiaryProfileViewModel.LoadAllDataAsync)
`HistoryEntry` is a display-only record — NOT stored in DB. Built at runtime from:
- Sessions → "جلسة" entries (linked via Session.BeneficiaryId)
- TherapistReports → "تقرير يومي" with mastery = OverallRating*20%
- Assessments → "تقييم" with StandardScore as mastery
- ObjectiveProgress → "تقدم الأهداف" with mastery = (Score/Target)*100

## Important XAML constraints
- `MaterialIcon.Kind` must be a compile-time enum literal — cannot bind to a string property.
- `SolidColorBrush Color="{Binding ...}"` doesn't work; use StaticResource brushes instead.
- All DataTemplates in Attachments tab use `$parent[ItemsControl].DataContext.CommandName` for commands.

## Opened by
`BeneficiariesViewModel.OpenProfileCommand` → loads full beneficiary with all Includes → creates `BeneficiaryProfileViewModel` → sets `IsProfileOpen = true`.
