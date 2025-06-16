namespace Llamkay.Services;

public interface ITheMuseService
{
    /// <summary>
    /// Gets a list of jobs based on the specified filters.
    /// </summary>
    /// <param name="page">Page number (optional).</param>
    /// <param name="category">Job category (optional).</param>
    /// <param name="level">Job level (optional).</param>
    /// <param name="location">Location (optional).</param>
    /// <param name="company">Company ID (optional).</param>
    /// <param name="descending">Descending order (optional).</param>
    /// <returns>Response containing the list of jobs.</returns>
    Task<TheMuseJobsResponse> GetJobsAsync(
        int? page = null,
        string? category = null,
        string? level = null,
        string? location = null,
        string? company = null,
        bool? descending = null);

    /// <summary>
    /// Gets the details of a specific job by its ID.
    /// </summary>
    /// <param name="jobId">Job ID.</param>
    /// <returns>Job details.</returns>
    Task<TheMuseJob> GetJobByIdAsync(string jobId);

    /// <summary>
    /// Gets a list of companies based on the specified filters.
    /// </summary>
    /// <param name="page">Page number (optional).</param>
    /// <param name="location">Location (optional).</param>
    /// <param name="industry">Industry (optional).</param>
    /// <param name="size">Company size (optional).</param>
    /// <returns>Response containing the list of companies.</returns>
    Task<TheMuseCompaniesResponse> GetCompaniesAsync(
        int? page = null,
        string? location = null,
        string? industry = null,
        string? size = null);

    /// <summary>
    /// Gets the details of a specific company by its ID.
    /// </summary>
    /// <param name="companyId">Company ID.</param>
    /// <returns>Company details.</returns>
    Task<TheMuseCompany> GetCompanyByIdAsync(string companyId);

    /// <summary>
    /// Gets jobs for a specific company.
    /// </summary>
    /// <param name="companyId">Company ID.</param>
    /// <param name="page">Page number (optional).</param>
    /// <param name="category">Job category (optional).</param>
    /// <param name="level">Job level (optional).</param>
    /// <returns>Response containing the list of jobs for the company.</returns>
    Task<TheMuseJobsResponse> GetJobsByCompanyAsync(
        string companyId,
        int? page = null,
        string? category = null,
        string? level = null);

    /// <summary>
    /// Searches for jobs by keyword.
    /// </summary>
    /// <param name="keyword">Search keyword.</param>
    /// <param name="page">Page number (optional).</param>
    /// <param name="location">Location (optional).</param>
    /// <param name="category">Category (optional).</param>
    /// <returns>Response containing the list of matching jobs.</returns>
    Task<TheMuseJobsResponse> SearchJobsAsync(
        string keyword,
        int? page = null,
        string? location = null,
        string? category = null);

    /// <summary>
    /// Gets the available job categories.
    /// </summary>
    /// <returns>List of job categories.</returns>
    Task<List<string>> GetJobCategoriesAsync();

    /// <summary>
    /// Gets the available job levels.
    /// </summary>
    /// <returns>List of job levels.</returns>
    Task<List<string>> GetJobLevelsAsync();

    /// <summary>
    /// Gets the most popular locations.
    /// </summary>
    /// <returns>List of popular locations.</returns>
    Task<List<TheMuseLocation>> GetPopularLocationsAsync();

    /// <summary>
    /// Checks if the service is available.
    /// </summary>
    /// <returns>True if the service responds correctly.</returns>
    Task<bool> IsServiceAvailableAsync();
}
