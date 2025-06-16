namespace Llamkay.Models;

public class TheMuseJobsResponse
{
    [JsonPropertyName("results")]
    public List<TheMuseJob> Results { get; set; } = new List<TheMuseJob>();

    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}
