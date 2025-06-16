namespace Llamkay.Models;

public class TheMuseCompany
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    // Changed from string to object to handle any type of value (object, array, or string)
    [JsonPropertyName("size")]
    public object Size { get; set; }

    [JsonPropertyName("model_type")]
    public string ModelType { get; set; }

    [JsonPropertyName("locations")]
    public List<TheMuseLocation> Locations { get; set; } = new List<TheMuseLocation>();

    [JsonPropertyName("industries")]
    public List<TheMuseIndustry> Industries { get; set; } = new List<TheMuseIndustry>();

    [JsonPropertyName("refs")]
    public TheMuseCompanyRefs Refs { get; set; }
}
