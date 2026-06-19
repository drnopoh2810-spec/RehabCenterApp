using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class BeneficiaryFormViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly Beneficiary? _existingBeneficiary;
    private readonly Action _onSaved;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private DateTime _dateOfBirth = DateTime.Now.AddYears(-10);
    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => this.RaiseAndSetIfChanged(ref _dateOfBirth, value);
    }

    private string _gender = "Male";
    public string Gender
    {
        get => _gender;
        set => this.RaiseAndSetIfChanged(ref _gender, value);
    }

    private string? _nationalId;
    public string? NationalId
    {
        get => _nationalId;
        set => this.RaiseAndSetIfChanged(ref _nationalId, value);
    }

    private string? _address;
    public string? Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    private string? _phone;
    public string? Phone
    {
        get => _phone;
        set => this.RaiseAndSetIfChanged(ref _phone, value);
    }

    private string _disabilityType = string.Empty;
    public string DisabilityType
    {
        get => _disabilityType;
        set => this.RaiseAndSetIfChanged(ref _disabilityType, value);
    }

    private string? _diagnosis;
    public string? Diagnosis
    {
        get => _diagnosis;
        set => this.RaiseAndSetIfChanged(ref _diagnosis, value);
    }

    private string? _guardianName;
    public string? GuardianName
    {
        get => _guardianName;
        set => this.RaiseAndSetIfChanged(ref _guardianName, value);
    }

    private string? _guardianPhone;
    public string? GuardianPhone
    {
        get => _guardianPhone;
        set => this.RaiseAndSetIfChanged(ref _guardianPhone, value);
    }

    private string? _insuranceCompany;
    public string? InsuranceCompany
    {
        get => _insuranceCompany;
        set => this.RaiseAndSetIfChanged(ref _insuranceCompany, value);
    }

    private string? _insuranceNumber;
    public string? InsuranceNumber
    {
        get => _insuranceNumber;
        set => this.RaiseAndSetIfChanged(ref _insuranceNumber, value);
    }

    private string _status = "Active";
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public bool IsEditing => _existingBeneficiary != null;

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public BeneficiaryFormViewModel(DatabaseService dbService, Beneficiary? beneficiary, Action onSaved)
    {
        _dbService = dbService;
        _existingBeneficiary = beneficiary;
        _onSaved = onSaved;

        if (beneficiary != null)
        {
            Name = beneficiary.Name;
            DateOfBirth = beneficiary.DateOfBirth;
            Gender = beneficiary.Gender;
            NationalId = beneficiary.NationalId;
            Address = beneficiary.Address;
            Phone = beneficiary.Phone;
            DisabilityType = beneficiary.DisabilityType;
            Diagnosis = beneficiary.Diagnosis;
            GuardianName = beneficiary.GuardianName;
            GuardianPhone = beneficiary.GuardianPhone;
            InsuranceCompany = beneficiary.InsuranceCompany;
            InsuranceNumber = beneficiary.InsuranceNumber;
            Status = beneficiary.Status;
        }

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => _onSaved());
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return;

        var beneficiary = _existingBeneficiary ?? new Beneficiary();
        beneficiary.Name = Name;
        beneficiary.DateOfBirth = DateOfBirth;
        beneficiary.Gender = Gender;
        beneficiary.NationalId = NationalId;
        beneficiary.Address = Address;
        beneficiary.Phone = Phone;
        beneficiary.DisabilityType = DisabilityType;
        beneficiary.Diagnosis = Diagnosis;
        beneficiary.GuardianName = GuardianName;
        beneficiary.GuardianPhone = GuardianPhone;
        beneficiary.InsuranceCompany = InsuranceCompany;
        beneficiary.InsuranceNumber = InsuranceNumber;
        beneficiary.Status = Status;

        if (_existingBeneficiary != null)
            await _dbService.UpdateBeneficiaryAsync(beneficiary);
        else
            await _dbService.AddBeneficiaryAsync(beneficiary);

        _onSaved();
    }
}