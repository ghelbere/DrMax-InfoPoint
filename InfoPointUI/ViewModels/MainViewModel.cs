using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfoPoint.Models;
using InfoPointUI.Services;
using InfoPointUI.Services.Interfaces;
using InfoPointUI.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace InfoPointUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // 📦 Colecții
    public ObservableCollection<ProductDto> Products { get; set; } = new();

    // 🔍 Search & Category 
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchCommand))]
    private string _searchTerm = "";

    [ObservableProperty]
    private string? _selectedCategory = "Toate";

    // 📄 Paginare - proprietăți care NU folosesc ObservableProperty
    // (pentru că au logică custom în setter)
    public int PageSize { get; set; } = 28;

    private int _currentPage = 0;
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                _ = SearchAsync();
                OnPropertyChanged(nameof(DisplayPage));
                OnPropertyChanged(nameof(IsLastPage));
            }
        }
    }

    private int _totalPages = 1;
    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (SetProperty(ref _totalPages, value <= 0 ? 1 : value))
            {
                OnPropertyChanged(nameof(IsLastPage));
            }
        }
    }

    // 💳 Card Properties - Astea merg cu ObservableProperty
    [ObservableProperty]
    private bool _isCardScanned = false;

    [ObservableProperty]
    private string _cardStatus = "Aștept scanare card...";

    [ObservableProperty]
    private string _cardNumber = string.Empty;

    // 🔧 Calculated Properties
    public int DisplayPage => (TotalPages < 1) ? 0 : CurrentPage + 1;
    public bool IsLastPage => DisplayPage >= TotalPages;

    // ⚙️ Services & Timers
    private readonly System.Timers.Timer _debounceTimer;
    private readonly ApiService _api = new();
    private readonly ICardService _cardService;
    private const string TabletId = "TAB-999";

    // 🎛️ Commands
    public ICommand SearchCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand ScanCardCommand { get; }

    public MainViewModel(ICardService cardService)
    {
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));

        // ⏱️ Debounce Timer pentru search
        _debounceTimer = new System.Timers.Timer(500);
        _debounceTimer.Elapsed += async (_, _) => await SearchAsync();
        _debounceTimer.AutoReset = false;

        // 🎯 Initializează Commands
        SearchCommand = new AsyncRelayCommand(
            SearchAsync,
            () => !string.IsNullOrWhiteSpace(SearchTerm)
        );

        SelectCategoryCommand = new RelayCommand<string>(category =>
        {
            SelectedCategory = category;
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                (Application.Current.MainWindow as MainWindow)?.FocusSearchBox();
            }, DispatcherPriority.ContextIdle);
        });

        NextPageCommand = new RelayCommand(
            () => CurrentPage++,
            () => Products.Count >= PageSize
        );

        PreviousPageCommand = new RelayCommand(
            () => { if (CurrentPage > 0) CurrentPage--; },
            () => CurrentPage > 0
        );

        ScanCardCommand = new RelayCommand(() =>
        {
            var cardWindow = App.Current.GetService<CardScanWindow>();
            cardWindow.Owner = Application.Current.MainWindow;
            cardWindow.ShowDialog();
        });

        // 🔔 Abonează-te la evenimente CardService
        _cardService.CardValidated += OnCardValidated;
        _cardService.CardValidationFailed += OnCardValidationFailed;

        // 📊 Initializează cu starea curentă
        if (_cardService.CurrentCardValidation?.IsValid == true)
        {
            UpdateFromCardService();
        }
    }

    // 🔄 Partial Methods pentru ObservableProperty changed events
    partial void OnSearchTermChanged(string value)
    {
        CurrentPage = 0;
        _debounceTimer.Stop();
        _debounceTimer.Start();
        ((AsyncRelayCommand)SearchCommand).NotifyCanExecuteChanged();
    }

    partial void OnSelectedCategoryChanged(string? value)
    {
        CurrentPage = 0;
        _ = SearchAsync();
    }

    // 💳 Card Event Handlers
    private void OnCardValidated(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(UpdateFromCardService);
    }

    private void OnCardValidationFailed(object? sender, string errorMessage)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsCardScanned = false;
            CardStatus = $"✗ {errorMessage}";
            CardNumber = string.Empty;
        });
    }

    private void UpdateFromCardService()
    {
        var validation = _cardService.CurrentCardValidation;
        if (validation?.IsValid == true)
        {
            CardNumber = validation.CardNumber ?? String.Empty;
            IsCardScanned = true;
            CardStatus = $"✓ Card: {validation.CardNumber}"; // GDPR friendly, no client name
        }
        else
        {
            CardNumber = String.Empty;
            IsCardScanned = false;
            CardStatus = String.Empty;
        }
    }

    [RelayCommand]
    private void ClearCard()
    {
        _cardService.ClearCard();
        UpdateFromCardService();
    }

    // 🔍 Search Logic
    public async Task SearchAsync()
    {
        var result = await _api.SearchProductsPagedAsync(
            SearchTerm,
            TabletId,
            SelectedCategory == "Toate" ? null : SelectedCategory,
            CurrentPage,
            PageSize);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Products.Clear();
            foreach (var product in result.Items)
            {
                Products.Add(product);
                _ = LoadProductImageAsync(product); // fire-and-forget
            }

            TotalPages = result.TotalPages;

            // Updatează CanExecute pentru butoanele de paginare
            ((RelayCommand)NextPageCommand).NotifyCanExecuteChanged();
            ((RelayCommand)PreviousPageCommand).NotifyCanExecuteChanged();
        });
    }

    // 🖼️ Image Loading
    private async Task LoadProductImageAsync(ProductDto product)
    {
        ImageSource? image = null;

        // 1️⃣ Verifică StaticResource
        if (Application.Current.Resources.Contains(product.ImageUrl.ToLower()))
        {
            try
            {
                var uriString = Application.Current.Resources[product.ImageUrl.ToLower()] as string;
                if (!string.IsNullOrEmpty(uriString))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(uriString, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    image = bmp;
                }
            }
            catch
            {
                image = null;
            }
        }

        // 2️⃣ Încarcă din URL
        if (image == null && Uri.TryCreate(product.ImageUrl, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            try
            {
                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(uri);
                using var ms = new MemoryStream(bytes);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                image = bmp;
            }
            catch
            {
                // Ignoră eroarea
            }
        }

        // 3️⃣ Fallback
        if (image == null)
        {
            image = new BitmapImage(new Uri("pack://application:,,,/InfoPointUI;component/Assets/Images/empty_image.png"));
        }

        product.ProductImage = image;
    }

    // 🧹 Cleanup
    ~MainViewModel()
    {
        if (_cardService != null)
        {
            _cardService.CardValidated -= OnCardValidated;
            _cardService.CardValidationFailed -= OnCardValidationFailed;
        }
        _debounceTimer?.Dispose();
    }
}