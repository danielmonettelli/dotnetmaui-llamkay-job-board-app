namespace Llamkay.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ITheMuseService _theMuseService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready to search jobs";

    [ObservableProperty]
    private string searchKeyword = string.Empty;

    [ObservableProperty]
    private string selectedLocation = string.Empty;

    [ObservableProperty]
    private string selectedCategory = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasJobs))]
    private ObservableCollection<TheMuseJob> jobs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCompanies))]
    private ObservableCollection<TheMuseCompany> companies;

    [ObservableProperty]
    private ObservableCollection<string> categories;

    [ObservableProperty]
    private ObservableCollection<string> locations;

    public MainViewModel(ITheMuseService theMuseService, ILogger<MainViewModel> logger)
    {
        _theMuseService = theMuseService ?? throw new ArgumentNullException(nameof(theMuseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        jobs = new ObservableCollection<TheMuseJob>();
        companies = new ObservableCollection<TheMuseCompany>();
        categories = new ObservableCollection<string>();
        locations = new ObservableCollection<string>();

        // Load initial data
        _ = Task.Run(LoadInitialDataAsync);
    }

    public bool HasJobs => Jobs.Count > 0;
    public bool HasCompanies => Companies.Count > 0;

    [RelayCommand]
    private async Task SearchJobs()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Searching jobs...";

            // Clear previous results
            Jobs.Clear();

            // Prepare search parameters
            var location = string.IsNullOrWhiteSpace(SelectedLocation) ? null : SelectedLocation;
            var category = string.IsNullOrWhiteSpace(SelectedCategory) ? null : SelectedCategory;

            TheMuseJobsResponse response;

            // Search with or without keyword
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                response = await _theMuseService.SearchJobsAsync(SearchKeyword, 1, location, category);
            }
            else
            {
                response = await _theMuseService.GetJobsAsync(1, category, null, location);
            }

            // Add jobs to collection
            if (response?.Results != null)
            {
                foreach (var job in response.Results)
                {
                    Jobs.Add(job);
                }

                StatusMessage = $"Found {Jobs.Count} jobs";

                // Extract unique locations for filter
                var uniqueLocations = response.Results
                    .SelectMany(j => j.Locations ?? new List<TheMuseLocation>())
                    .Select(l => l.Name)
                    .Distinct()
                    .Where(l => !string.IsNullOrEmpty(l))
                    .Take(20)
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Locations.Clear();
                    foreach (var loc in uniqueLocations)
                    {
                        Locations.Add(loc);
                    }
                });
            }
            else
            {
                StatusMessage = "No jobs found";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching jobs");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadCompanies()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Loading companies...";

            Companies.Clear();

            var response = await _theMuseService.GetCompaniesAsync(1);

            if (response?.Results != null)
            {
                foreach (var company in response.Results)
                {
                    Companies.Add(company);
                }

                StatusMessage = $"Loaded {Companies.Count} companies";
            }
            else
            {
                StatusMessage = "No companies found";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading companies");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TestApi()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Testing The Muse API...";

            var isAvailable = await _theMuseService.IsServiceAvailableAsync();

            if (isAvailable)
            {
                StatusMessage = "✅ The Muse API working correctly";

                // Perform a test search
                await Task.Delay(1000);
                SearchKeyword = "developer";
                await SearchJobs();
            }
            else
            {
                StatusMessage = "❌ The Muse API not available";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing API");
            StatusMessage = $"❌ Error testing API: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshData()
    {
        ClearResults();
        await LoadInitialDataAsync();
        StatusMessage = "Data refreshed";
    }

    [RelayCommand]
    private void ClearResults()
    {
        Jobs.Clear();
        Companies.Clear();
        SearchKeyword = string.Empty;
        SelectedLocation = string.Empty;
        SelectedCategory = string.Empty;
        StatusMessage = "Results cleared";
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            StatusMessage = "Loading categories...";

            // Load categories
            var categories = await _theMuseService.GetJobCategoriesAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            });

            StatusMessage = "Initial data loaded";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading initial data");
            StatusMessage = $"Error loading initial data: {ex.Message}";
        }
    }
}