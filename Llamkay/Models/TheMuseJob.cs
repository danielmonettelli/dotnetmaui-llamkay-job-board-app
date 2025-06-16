namespace Llamkay.Models;

public class TheMuseJob
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("contents")]
    public string Contents { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("publication_date")]
    public DateTime PublicationDate { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }

    [JsonPropertyName("model_type")]
    public string ModelType { get; set; }

    [JsonPropertyName("company")]
    public TheMuseCompany Company { get; set; }

    [JsonPropertyName("categories")]
    public List<TheMuseCategory> Categories { get; set; } = new List<TheMuseCategory>();

    [JsonPropertyName("levels")]
    public List<TheMuseLevel> Levels { get; set; } = new List<TheMuseLevel>();

    [JsonPropertyName("locations")]
    public List<TheMuseLocation> Locations { get; set; } = new List<TheMuseLocation>();

    [JsonPropertyName("tags")]
    public List<TheMuseTag> Tags { get; set; } = new List<TheMuseTag>();

    [JsonPropertyName("refs")]
    public TheMuseJobRefs Refs { get; set; }
}
