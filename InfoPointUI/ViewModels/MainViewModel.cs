using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using InfoPointUI.Commands;
using InfoPointUI.Models;
using InfoPointUI.Services;

namespace InfoPointUI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ProductDto> Products { get; set; } = new();

    private string _searchTerm = "";
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm != value)
            {
                _searchTerm = value;
                CurrentPage = 0;
                OnPropertyChanged();
                debounceTimer.Stop();
                debounceTimer.Start();
                ((RelayCommand<object>)SearchCommand).RaiseCanExecuteChanged();
            }
        }
    }

    private string? _selectedCategory = "Toate";
    public string? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                CurrentPage = 0;
                OnPropertyChanged();
                _ = SearchAsync();
            }
        }
    }

    // 🔢 Paginare
    public int PageSize { get; set; } = 15;

    private int _currentPage = 0;
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged();
                _ = SearchAsync();
                OnPropertyChanged(nameof(DisplayPage));
                OnPropertyChanged(nameof(IsLastPage));
            }
        }
    }

    public int DisplayPage => (TotalPages<1)?0:CurrentPage + 1;

    private int _totalPages = 1;
    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (_totalPages != value)
            {
                _totalPages = value <= 0 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLastPage)); // ne asigurăm că se reevaluează
            }
        }
    }


    public bool IsLastPage => DisplayPage >= TotalPages;

    public ICommand SearchCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    private readonly System.Timers.Timer debounceTimer;
    private readonly ApiService _api = new();
    private readonly string TabletId = "TAB-999";

    public MainViewModel()
    {
        TotalPages = 1;         // sau 0, dar ideal e să fie > 0

        // Debounce timer for searching
        debounceTimer = new(150);
        debounceTimer.Elapsed += async (_, _) => await SearchAsync();
        debounceTimer.AutoReset = false;

        SearchCommand = new RelayCommand<object>(
            async _ => await SearchAsync(),
            _ => !string.IsNullOrWhiteSpace(SearchTerm)
        );

        SelectCategoryCommand = new RelayCommand<string>(category =>
        {
            SelectedCategory = category;
        });

        NextPageCommand = new RelayCommand<object>(_ =>
        {
            if (Products.Count < PageSize)
            {
                return;
            }

            CurrentPage++;
        });

        PreviousPageCommand = new RelayCommand<object>(_ =>
        {
            if (CurrentPage > 0)
                CurrentPage--;
        });
    }

    public async Task SearchAsync()
    {
        string? categoryParam = SelectedCategory == "Toate" ? null : SelectedCategory;

        var result = await _api.SearchProductsPagedAsync(
            SearchTerm,
            TabletId,
            categoryParam,
            CurrentPage,
            PageSize);

        Application.Current.Dispatcher.Invoke(() =>
        {
            // daca nu se actualizeaza Products in acest fel, binding-urile din interfata nu functioneaza
            Products.Clear();
            foreach (var product in result.Items)
                Products.Add(product);

            TotalPages = result.TotalPages; // 👈 actualizat din server
        });
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new(name));
}
