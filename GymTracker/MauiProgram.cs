using Microsoft.Extensions.Logging;
using GymTracker.Data;
using GymTracker.Services;
using Plugin.Maui.Audio;

namespace GymTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(
                        "OpenSans-Regular.ttf",
                        "OpenSansRegular");
                });

            // MAUI Blazor Hybrid
            builder.Services.AddMauiBlazorWebView();

            // Local database
            builder.Services.AddSingleton<AppDatabase>();

            // Your existing database service
            builder.Services.AddSingleton<DatabaseService>();

            // Audio
            builder.Services.AddSingleton(
                AudioManager.Current);

#if ANDROID

            // Native Android rest timer alarm
            builder.Services.AddSingleton<
                GymTracker.Platforms.Android
                    .AndroidRestAlarmService>();

#endif

#if DEBUG

            builder.Services
                .AddBlazorWebViewDeveloperTools();

            builder.Logging.AddDebug();

#endif

            return builder.Build();
        }
    }
}