namespace Llamkay.Models;

public class TheMuseTag
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }
}
