namespace Llamkay.Models;

public class TheMuseLevel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }
}
