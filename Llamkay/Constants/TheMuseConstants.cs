namespace Llamkay.Constants;

public static class TheMuseConstants
{
    // Base URL for The Muse API V2
    public const string BaseUrl = "https://www.themuse.com/api/public/";

    // Main endpoints
    public const string JobsEndpoint = "jobs";
    public const string CompaniesEndpoint = "companies";
    public const string CoachingEndpoint = "coaching";

    // Common query parameters
    public const string PageParameter = "page";
    public const string CategoryParameter = "category";
    public const string LevelParameter = "level";
    public const string LocationParameter = "location";
    public const string CompanyParameter = "company";
    public const string DescendingParameter = "descending";

    // Default values
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int DefaultTimeout = 30; // seconds    // API Key - The Muse API Key
    public const string ApiKey = "THE_MUSE_API_KEY_HERE";
    public const string ApiKeyParameter = "api_key";
    public const string ApiKeyHeader = "X-API-Key";

    // HTTP Headers
    public const string UserAgentHeader = "User-Agent";
    public const string UserAgentValue = "Llankay-MAUI/1.0";
    public const string ContentTypeHeader = "Content-Type";
    public const string ContentTypeJson = "application/json";
    public const string AuthorizationHeader = "Authorization";

    // Response codes
    public const int SuccessStatusCode = 200;
    public const int NotFoundStatusCode = 404;
    public const int TooManyRequestsStatusCode = 429;

    // Job filters
    public static class JobFilters
    {
        public const string EntryLevel = "Entry Level";
        public const string MidLevel = "Mid Level";
        public const string SeniorLevel = "Senior Level";
        public const string Internship = "Internship";
    }

    // Common job categories
    public static class JobCategories
    {
        public const string Engineering = "Engineering";
        public const string Design = "Design";
        public const string Marketing = "Marketing";
        public const string Sales = "Sales";
        public const string ProductManagement = "Product Management";
        public const string DataScience = "Data Science";
        public const string Finance = "Finance";
        public const string Operations = "Operations";
        public const string CustomerService = "Customer Service";
        public const string HumanResources = "Human Resources";
    }

    // Cache configuration
    public const int CacheExpirationMinutes = 15;
    public const string CacheKeyPrefix = "themuse_";
}
