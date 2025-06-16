namespace Llamkay.Models;

public class TheMuseCompaniesResponse
{
    [JsonPropertyName("results")]
    public List<TheMuseCompany> Results { get; set; } = new List<TheMuseCompany>();

    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}
