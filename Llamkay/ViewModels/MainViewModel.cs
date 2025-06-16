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
            StatusMessage = "🔄 Making direct request to The Muse API...";

            Companies.Clear();

            // Create a test company to verify if the issue is visualization-related
            var testCompany = new TheMuseCompany
            {
                Id = 999,
                Name = "TEST COMPANY - IGNORE THIS",
                Description = "This is a test company to verify if the CollectionView works correctly",
                ShortName = "TEST"
            };

            // Add the test company at the beginning to verify visualization
            Companies.Add(testCompany);

            // Get direct URL for diagnostics
            string apiUrl = TheMuseConstants.BaseUrl + TheMuseConstants.CompaniesEndpoint;

            // Try a direct HTTP request with HttpClient for diagnostics
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Add a User-Agent to simulate a browser (some APIs require it)
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 MAUI Diagnostics Client");

                // IMPORTANT: The API expects page to start at 1, not 0
                string fullUrl = $"{apiUrl}?page=1&api_key={TheMuseConstants.ApiKey}";
                StatusMessage = $"Requesting data from: {fullUrl}";

                try
                {
                    // Make direct GET request
                    var directResponse = await httpClient.GetAsync(fullUrl);

                    // Log detailed response information
                    _logger.LogInformation($"HTTP status code: {(int)directResponse.StatusCode} ({directResponse.StatusCode})");
                    _logger.LogInformation($"Response headers: {string.Join(", ", directResponse.Headers.Select(h => $"{h.Key}:{string.Join(",", h.Value)}"))}");

                    // Read response content directly
                    var jsonContent = await directResponse.Content.ReadAsStringAsync();

                    // Create a company that shows the JSON response for diagnostics
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        var jsonPreviewCompany = new TheMuseCompany
                        {
                            Id = 998,
                            Name = $"DIRECT JSON RESPONSE (Status: {directResponse.StatusCode})",
                            Description = jsonContent.Length > 500 ?
                                jsonContent.Substring(0, 500) + "..." :
                                jsonContent
                        };

                        Companies.Add(jsonPreviewCompany);

                        // Try to manually deserialize the response
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                            };

                            var manualResponse = JsonSerializer.Deserialize<TheMuseCompaniesResponse>(jsonContent, options);

                            if (manualResponse?.Results != null && manualResponse.Results.Count > 0)
                            {
                                // Show a summary of manual deserialization
                                var summaryCompany = new TheMuseCompany
                                {
                                    Id = 997,
                                    Name = $"DESERIALIZATION SUMMARY: {manualResponse.Results.Count} companies found",
                                    Description = $"First company: {manualResponse.Results[0].Name}"
                                };

                                Companies.Add(summaryCompany);

                                // Add the first companies from the real response
                                foreach (var company in manualResponse.Results)
                                {
                                    company.Name = $"[REAL API] {company.Name}";
                                    Companies.Add(company);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Add a company that shows the deserialization error
                            var errorCompany = new TheMuseCompany
                            {
                                Id = 996,
                                Name = "DESERIALIZATION ERROR",
                                Description = ex.Message
                            };

                            Companies.Add(errorCompany);
                        }
                    }
                    else
                    {
                        var emptyResponseCompany = new TheMuseCompany
                        {
                            Id = 995,
                            Name = "EMPTY API RESPONSE",
                            Description = "The API returned an empty response"
                        };

                        Companies.Add(emptyResponseCompany);
                    }

                    StatusMessage = $"✅ HTTP request completed. Status: {directResponse.StatusCode}";
                }
                catch (Exception httpEx)
                {
                    // Add a company that shows the HTTP error
                    var httpErrorCompany = new TheMuseCompany
                    {
                        Id = 994,
                        Name = "DIRECT HTTP ERROR",
                        Description = httpEx.Message
                    };

                    Companies.Add(httpErrorCompany);

                    StatusMessage = $"❌ Direct HTTP error: {httpEx.Message}";
                    _logger.LogError(httpEx, "Error in direct HTTP request");
                }
            }

            // Also try with the normal service for comparison - FIXED WITH PAGE=1
            try
            {
                _logger.LogInformation("Also trying with TheMuseService (with page=1)...");
                StatusMessage += " | Also trying with normal service...";

                // IMPORTANT: We use 1 as initial page, not 0
                var response = await _theMuseService.GetCompaniesAsync(1);

                if (response?.Results != null && response.Results.Count > 0)
                {
                    var serviceCompany = new TheMuseCompany
                    {
                        Id = 993,
                        Name = $"VIA NORMAL SERVICE: {response.Results.Count} companies found",
                        Description = $"Page {response.Page} of {response.PageCount} has {response.Results.Count} companies"
                    };

                    Companies.Add(serviceCompany);

                    // Add companies from normal service
                    foreach (var company in response.Results)
                    {
                        var companyClone = new TheMuseCompany
                        {
                            Id = company.Id,
                            Name = $"[SERVICE] {company.Name}",
                            Description = company.Description,
                            ShortName = company.ShortName,
                            Size = company.Size,
                            ModelType = company.ModelType,
                            Industries = company.Industries,
                            Locations = company.Locations,
                            Refs = company.Refs
                        };

                        Companies.Add(companyClone);
                    }
                }
                else
                {
                    var noServiceDataCompany = new TheMuseCompany
                    {
                        Id = 992,
                        Name = "NORMAL SERVICE NO DATA",
                        Description = "TheMuseService returned no companies"
                    };

                    Companies.Add(noServiceDataCompany);
                }
            }
            catch (Exception serviceEx)
            {
                var serviceErrorCompany = new TheMuseCompany
                {
                    Id = 991,
                    Name = "NORMAL SERVICE ERROR",
                    Description = serviceEx.Message
                };

                Companies.Add(serviceErrorCompany);
                _logger.LogError(serviceEx, "Error using TheMuseService");
            }

            // Finalization
            StatusMessage = $"Diagnostics completed - {Companies.Count} items displayed";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error while diagnosing the API");
            StatusMessage = $"❌ General error: {ex.Message}";

            // Add an error company to make sure there's visible content
            var errorCompany = new TheMuseCompany
            {
                Id = 990,
                Name = "GENERAL ERROR",
                Description = ex.Message
            };

            Companies.Add(errorCompany);
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