namespace Llamkay;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        // Register HttpClient with configuration
        builder.Services.AddHttpClient<ITheMuseService, TheMuseService>(client =>
        {
            // Additional HttpClient configuration if necessary
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        // Register services
        builder.Services.AddScoped<ITheMuseService, TheMuseService>();
        // Register ViewModels
        builder.Services.AddScoped<MainViewModel>();
        // Register Pages
        builder.Services.AddScoped<MainPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}