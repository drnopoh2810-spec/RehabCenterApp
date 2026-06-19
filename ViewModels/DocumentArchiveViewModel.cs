using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class DocumentArchiveViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<DocumentArchive> _documents = new();
    public ObservableCollection<DocumentArchive> Documents
    {
        get => _documents;
        set => this.RaiseAndSetIfChanged(ref _documents, value);
    }

    private DocumentArchive? _selectedDocument;
    public DocumentArchive? SelectedDocument
    {
        get => _selectedDocument;
        set => this.RaiseAndSetIfChanged(ref _selectedDocument, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchQuery, value);
            _ = SearchAsync();
        }
    }

    private string _filterCategory = "All";
    public string FilterCategory
    {
        get => _filterCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterCategory, value);
            _ = LoadAsync();
        }
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> OcrCommand { get; }

    public DocumentArchiveViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync);
        AddCommand = ReactiveCommand.Create(() => { });
        DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedDocument != null)
            {
                var confirm = await _dialogService.ShowConfirmAsync("حذف", "هل أنت متأكد؟");
                if (confirm)
                {
                    await _dbService.DeleteDocumentArchiveAsync(SelectedDocument.Id);
                    await LoadAsync();
                }
            }
        });
        OcrCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedDocument != null)
            {
                // Simulate OCR
                SelectedDocument.OcrText = "نص مستخرج من المستند...";
                SelectedDocument.IsIndexed = true;
                await _dbService.UpdateDocumentArchiveAsync(SelectedDocument);
                await _dialogService.ShowInfoAsync("OCR", "تم استخراج النص بنجاح");
                await LoadAsync();
            }
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetDocumentArchivesAsync(FilterCategory == "All" ? null : FilterCategory);
        Documents.Clear();
        foreach (var d in list) Documents.Add(d);
    }

    private async Task SearchAsync()
    {
        var list = await _dbService.SearchDocumentArchivesAsync(SearchQuery);
        Documents.Clear();
        foreach (var d in list) Documents.Add(d);
    }
}