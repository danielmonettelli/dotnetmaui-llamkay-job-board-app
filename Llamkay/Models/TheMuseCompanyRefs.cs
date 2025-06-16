namespace Llamkay.Models;

public class TheMuseCompanyRefs
{
    [JsonPropertyName("landing_page")]
    public string LandingPage { get; set; }

    [JsonPropertyName("logo_image")]
    public string LogoImage { get; set; }

    [JsonPropertyName("f1_image")]
    public string F1Image { get; set; }
}
