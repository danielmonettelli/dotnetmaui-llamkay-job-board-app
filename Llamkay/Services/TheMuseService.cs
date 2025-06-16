namespace Llamkay.Services;

public class TheMuseService : ITheMuseService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TheMuseService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _apiKey;

    public TheMuseService(HttpClient httpClient, ILogger<TheMuseService> logger, IConfiguration configuration = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Get API Key from configuration or environment variables
        _apiKey = configuration?["TheMuseApiKey"] ??
                 Environment.GetEnvironmentVariable("THEMUSE_API_KEY") ??
                 TheMuseConstants.ApiKey;

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("The Muse API Key not configured. Some functionalities may be limited");
        }
        else
        {
            _logger.LogInformation("The Muse API Key configured successfully");
        }

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(TheMuseConstants.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add(TheMuseConstants.UserAgentHeader, TheMuseConstants.UserAgentValue);
        _httpClient.Timeout = TimeSpan.FromSeconds(TheMuseConstants.DefaultTimeout);

        // Add API Key to header if available
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add(TheMuseConstants.ApiKeyHeader, _apiKey);
        }

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<TheMuseJobsResponse> GetJobsAsync(
        int? page = null,
        string? category = null,
        string? level = null,
        string? location = null,
        string? company = null,
        bool? descending = null)
    {
        try
        {
            var queryParams = BuildQueryParameters(new Dictionary<string, object?>
                {
                    { TheMuseConstants.PageParameter, page },
                    { TheMuseConstants.CategoryParameter, category },
                    { TheMuseConstants.LevelParameter, level },
                    { TheMuseConstants.LocationParameter, location },
                    { TheMuseConstants.CompanyParameter, company },
                    { TheMuseConstants.DescendingParameter, descending?.ToString().ToLower() }
                });

            var requestUri = $"{TheMuseConstants.JobsEndpoint}{queryParams}";
            _logger.LogInformation($"Requesting jobs: {requestUri}");

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            return DeserializeResponse<TheMuseJobsResponse>(jsonContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error while fetching jobs from The Muse API");
            throw new Exception("Connection error with The Muse API", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error while deserializing jobs response");
            throw new Exception("Error while processing data from The Muse API", ex);
        }
    }

    public async Task<TheMuseJob> GetJobByIdAsync(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be empty", nameof(jobId));

        try
        {
            var requestUri = $"{TheMuseConstants.JobsEndpoint}/{jobId}";
            _logger.LogInformation($"Requesting job by ID: {requestUri}");

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            return DeserializeResponse<TheMuseJob>(jsonContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error while fetching job by ID {jobId}");
            throw new Exception($"Error while fetching job by ID {jobId}", ex);
        }
    }

    public async Task<TheMuseCompaniesResponse> GetCompaniesAsync(
        int? page = null,
        string? location = null,
        string? industry = null,
        string? size = null)
    {
        try
        {
            // Ensure that page is always at least 1
            // The Muse API expects pagination to start at 1, not 0
            if (page.HasValue && page.Value < 1)
            {
                page = 1;
                _logger.LogWarning("The page parameter was adjusted to 1 because the API expects values >= 1");
            }
            else if (!page.HasValue)
            {
                page = 1; // Default value if not specified
            }

            var queryParams = BuildQueryParameters(new Dictionary<string, object?>
                {
                    { TheMuseConstants.PageParameter, page },
                    { TheMuseConstants.LocationParameter, location },
                    { "industry", industry },
                    { "size", size }
                });

            var requestUri = $"{TheMuseConstants.CompaniesEndpoint}{queryParams}";
            _logger.LogInformation($"Requesting companies: {requestUri}");

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();

            // Save JSON for debugging
            var previewLength = Math.Min(200, jsonContent.Length);
            _logger.LogInformation($"API response preview: {jsonContent.Substring(0, previewLength)}...");

            // Try to deserialize directly
            try
            {
                var result = DeserializeResponse<TheMuseCompaniesResponse>(jsonContent);

                // Validate the result
                if (result != null && result.Results != null && result.Results.Count > 0)
                {
                    _logger.LogInformation($"Successful deserialization: {result.Results.Count} companies found");
                    return result;
                }
                else
                {
                    _logger.LogWarning("Deserialization was successful but no companies were found");
                    return new TheMuseCompaniesResponse { Results = new List<TheMuseCompany>() };
                }
            }
            catch (Exception deserEx)
            {
                _logger.LogError(deserEx, "Error during response deserialization");
                throw new Exception($"Error processing API response: {deserEx.Message}", deserEx);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error while fetching companies from The Muse API. Status: {ex.StatusCode}");
            throw new Exception($"Connection error with The Muse API: {ex.Message} (Status: {ex.StatusCode})", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error getting companies");
            throw new Exception($"General error getting companies: {ex.Message}", ex);
        }
    }

    public async Task<TheMuseCompany> GetCompanyByIdAsync(string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            throw new ArgumentException("Company ID must not be empty", nameof(companyId));

        try
        {
            var requestUri = $"{TheMuseConstants.CompaniesEndpoint}/{companyId}";
            _logger.LogInformation($"Requesting company by ID: {requestUri}");

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            return DeserializeResponse<TheMuseCompany>(jsonContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error while fetching company by ID {companyId}");
            throw new Exception($"Error while fetching company by ID {companyId}", ex);
        }
    }

    public async Task<TheMuseJobsResponse> GetJobsByCompanyAsync(
        string companyId,
        int? page = null,
        string? category = null,
        string? level = null)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            throw new ArgumentException("Company ID cannot be empty", nameof(companyId));

        return await GetJobsAsync(page, category, level, null, companyId);
    }

    public async Task<TheMuseJobsResponse> SearchJobsAsync(
        string keyword,
        int? page = null,
        string? location = null,
        string? category = null)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("Keyword cannot be empty", nameof(keyword));

        try
        {
            var queryParams = BuildQueryParameters(new Dictionary<string, object?>
                {
                    { TheMuseConstants.PageParameter, page },
                    { TheMuseConstants.LocationParameter, location },
                    { TheMuseConstants.CategoryParameter, category },
                    { "q", keyword }
                });

            var requestUri = $"{TheMuseConstants.JobsEndpoint}{queryParams}";
            _logger.LogInformation($"Searching jobs with keyword: {keyword}");

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            return DeserializeResponse<TheMuseJobsResponse>(jsonContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error while searching jobs with keyword: {keyword}");
            throw new Exception("Error during job search", ex);
        }
    }

    public async Task<List<string>> GetJobCategoriesAsync()
    {
        try
        {
            // The Muse API does not have a specific endpoint for categories,
            // so we return predefined categories
            await Task.Delay(1); // Simulate asynchronous call

            return new List<string>
                {
                    TheMuseConstants.JobCategories.Engineering,
                    TheMuseConstants.JobCategories.Design,
                    TheMuseConstants.JobCategories.Marketing,
                    TheMuseConstants.JobCategories.Sales,
                    TheMuseConstants.JobCategories.ProductManagement,
                    TheMuseConstants.JobCategories.DataScience,
                    TheMuseConstants.JobCategories.Finance,
                    TheMuseConstants.JobCategories.Operations,
                    TheMuseConstants.JobCategories.CustomerService,
                    TheMuseConstants.JobCategories.HumanResources
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching job categories");
            throw;
        }
    }

    public async Task<List<string>> GetJobLevelsAsync()
    {
        try
        {
            await Task.Delay(1); // Simulate async call

            return new List<string>
                {
                    TheMuseConstants.JobFilters.EntryLevel,
                    TheMuseConstants.JobFilters.MidLevel,
                    TheMuseConstants.JobFilters.SeniorLevel,
                    TheMuseConstants.JobFilters.Internship
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching job levels");
            throw;
        }
    }

    public async Task<List<TheMuseLocation>> GetPopularLocationsAsync()
    {
        try
        {
            // Retrieve a sample of jobs and extract unique locations
            var jobsResponse = await GetJobsAsync(page: 1);

            var locations = jobsResponse.Results
                .SelectMany(job => job.Locations ?? new List<TheMuseLocation>())
                .GroupBy(loc => loc.Name)
                .Select(g => g.First())
                .Take(20)
                .ToList();

            return locations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching popular locations");
            return new List<TheMuseLocation>();
        }
    }

    public async Task<bool> IsServiceAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(TheMuseConstants.JobsEndpoint + "?page=0");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking service availability");
            return false;
        }
    }

    private string BuildQueryParameters(Dictionary<string, object?> parameters)
    {
        var validParams = parameters
            .Where(kvp => kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()))
            .ToList();

        // Add API Key as a query parameter if available and not used in the header
        if (!string.IsNullOrWhiteSpace(_apiKey) && !_httpClient.DefaultRequestHeaders.Contains(TheMuseConstants.ApiKeyHeader))
        {
            validParams.Add(new KeyValuePair<string, object?>(TheMuseConstants.ApiKeyParameter, _apiKey));
        }

        if (!validParams.Any())
            return string.Empty;

        var queryString = string.Join("&", validParams.Select(kvp =>
            $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value?.ToString() ?? "")}"));

        return $"?{queryString}";
    }

    private T DeserializeResponse<T>(string jsonContent) where T : new()
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            _logger.LogWarning("Empty JSON response received");
            return new T();
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                // Allow more flexible readings
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                // Ignore unknown properties to be more tolerant to API changes
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var result = JsonSerializer.Deserialize<T>(jsonContent, options);

            if (result == null)
            {
                _logger.LogWarning($"Deserialization produced a NULL result for type {typeof(T).Name}");
                return new T();
            }

            return result;
        }
        catch (JsonException ex)
        {
            // Log error details to facilitate debugging
            _logger.LogError(ex, $"Error deserializing JSON response for type {typeof(T).Name}");
            _logger.LogError($"Error path: {ex.Path}, Line: {ex.LineNumber}, Position: {ex.BytePositionInLine}");

            // Show JSON fragment to help diagnose
            if (!string.IsNullOrEmpty(jsonContent))
            {
                var previewLength = Math.Min(500, jsonContent.Length);
                _logger.LogError($"JSON preview: {jsonContent.Substring(0, previewLength)}...");
            }

            return new T();
        }
    }
}