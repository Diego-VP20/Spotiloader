using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spotiloader.API;
using Spotiloader.Config;

namespace Spotiloader
{
    public static class Program
    {
        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(l => l.AddSerilog())
                .AddSingleton<SpotifyService>()
                .BuildServiceProvider();
        }

        private static void InitLoggerConfiguration()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }
        
        private static void InitSpotifyService(IServiceProvider serviceProvider, SpotifyApplication config)
        {
            var spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
            spotifyService.Init(config);
        }
        
        public static async Task Main()
        {
            // Init logger configuration
            InitLoggerConfiguration();
            
            // Configure services
            await using var serviceProvider = ConfigureServices();
            
            // Present program banner to user.
            PresentHeader();
            
            // Initialize config. (Create, Validate, Load...)
            var config = await ConfigManager.InitializeConfigAsync();
            
            // Initialize Spotify service class.
            InitSpotifyService(serviceProvider, config);
            
            // Present header again.
            PresentHeader(true);
            
            // Present menu to user.
            await new Menu.Menu(serviceProvider).Run();
            
            // Exit program.
            GracefulExit();
        }

        private static void GracefulExit()
        {
            AnsiConsole.MarkupLine("[green bold]Thank you for using Spotiloader![/]");
            Environment.Exit(0);
        }

        private static void PresentHeader(bool skipCredits = false)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Spotiloader").Centered().Color(Color.Green));
            if (!skipCredits)
            {
                AnsiConsole.Write(new Markup("[green]V1.0 by Diego-VP20[/]").Centered());
            }
        }
    }
}