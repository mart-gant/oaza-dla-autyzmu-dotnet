using Microsoft.Extensions.Logging;
using OazaDlaAutyzmu.Mobile.Services;

namespace OazaDlaAutyzmu.Mobile;

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
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // HttpClient setup. In the Android Emulator, 10.0.2.2 maps to localhost of the host machine.
        // For physical devices or other platforms, we default to localhost:7115,
        // but we will also store the URL in Preferences so users can customize it.
        string defaultUrl = DeviceInfo.Platform == DevicePlatform.Android 
            ? "https://10.0.2.2:7115" 
            : "https://localhost:7115";

        string backendUrl = Preferences.Default.Get("BackendUrl", defaultUrl);

        builder.Services.AddScoped(sp => 
        {
            var handler = new HttpClientHandler();
            
            // Bypass SSL certificate validation in development (necessary for localhost self-signed certs)
            #if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            #endif

            return new HttpClient(handler)
            {
                BaseAddress = new Uri(backendUrl)
            };
        });

        // Register ApiService
        builder.Services.AddScoped<ApiService>();

        return builder.Build();
    }
}
