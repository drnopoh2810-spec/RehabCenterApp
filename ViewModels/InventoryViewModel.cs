using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class InventoryViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<InventoryItem> _items = new();
    public ObservableCollection<InventoryItem> Items
    {
        get => _items;
        set => this.RaiseAndSetIfChanged(ref _items, value);
    }

    private InventoryItem? _selectedItem;
    public InventoryItem? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private string _itemName = string.Empty;
    public string ItemName
    {
        get => _itemName;
        set => this.RaiseAndSetIfChanged(ref _itemName, value);
    }

    private string _category = "Equipment";
    public string Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set => this.RaiseAndSetIfChanged(ref _quantity, value);
    }

    private int _minStock = 5;
    public int MinStock
    {
        get => _minStock;
        set => this.RaiseAndSetIfChanged(ref _minStock, value);
    }

    private decimal? _unitCost;
    public decimal? UnitCost
    {
        get => _unitCost;
        set => this.RaiseAndSetIfChanged(ref _unitCost, value);
    }

    private int _lowStockCount;
    public int LowStockCount
    {
        get => _lowStockCount;
        set => this.RaiseAndSetIfChanged(ref _lowStockCount, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> AddStockCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveStockCommand { get; }

    public InventoryViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.Create(() => IsFormOpen = true);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => IsFormOpen = false);
        AddStockCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedItem != null)
            {
                SelectedItem.Quantity += 1;
                await _dbService.UpdateInventoryItemAsync(SelectedItem);
                await LoadAsync();
            }
        });
        RemoveStockCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedItem != null && SelectedItem.Quantity > 0)
            {
                SelectedItem.Quantity -= 1;
                await _dbService.UpdateInventoryItemAsync(SelectedItem);
                await LoadAsync();
            }
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetInventoryItemsAsync();
        Items.Clear();
        foreach (var item in list) Items.Add(item);
        LowStockCount = list.Count(i => i.Quantity <= i.MinStockLevel);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ItemName)) return;

        var item = new InventoryItem
        {
            Name = ItemName,
            Category = Category,
            Quantity = Quantity,
            MinStockLevel = MinStock,
            UnitCost = UnitCost,
            Status = "Available"
        };

        await _dbService.AddInventoryItemAsync(item);
        IsFormOpen = false;
        ItemName = string.Empty;
        await LoadAsync();
    }
}