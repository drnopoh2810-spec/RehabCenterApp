using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class CorrespondenceViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    private ObservableCollection<Correspondence> _items = new();
    public ObservableCollection<Correspondence> Items
    {
        get => _items;
        set => this.RaiseAndSetIfChanged(ref _items, value);
    }

    private Correspondence? _selectedItem;
    public Correspondence? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }

    private string _filterType = "All";
    public string FilterType
    {
        get => _filterType;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterType, value);
            _ = LoadAsync();
        }
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public CorrespondenceViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.Create(() => { });
        DeleteCommand = ReactiveCommand.Create(() => { });
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        // Load from database
    }
}