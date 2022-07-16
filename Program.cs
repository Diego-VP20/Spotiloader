using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spotiloader.API;
using Spotiloader.Config;
using Swan.Logging;

namespace Spotiloader
{
    public static class Program
    {
        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(l => l.AddSerilog())
                .AddSingleton<YoutubeService>()
                .AddSingleton<SpotifyService>()
                .AddSingleton<Menu.Menu>()
                .BuildServiceProvider();
        }

        private static void InitLoggerConfiguration()
        {
            Logger.UnregisterLogger<ConsoleLogger>();
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }
        
        private static async Task<bool> InitSpotifyService(IServiceProvider serviceProvider, SpotifyApplication config)
        {
            var spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
            await spotifyService.Init(config);
            return spotifyService.IsAuthenticated();
        }
        
        public static async Task Main()
        {
            try
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
                var initSpotifyServiceSuccess = await InitSpotifyService(serviceProvider, config);

                // Present header again.
                PresentHeader(true);

                // Present menu to user.
                var menu = serviceProvider.GetRequiredService<Menu.Menu>();
            
                menu.IsAuthenticated = initSpotifyServiceSuccess;
            
                await menu.Run();

                // Exit program.
                GracefulExit();
            }
            catch (Exception e)
            {
                NonGracefulExit(e.Message);
            }
        }

        private static void GracefulExit()
        {
            AnsiConsole.MarkupLine("[green bold]Thank you for using Spotiloader![/]");
            Environment.Exit(0);
        }

        private static void NonGracefulExit(string message = "")
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[red bold]Spotiloader has encountered an error![/]");
            AnsiConsole.MarkupLine($"[red bold]{message}[/]");
            Environment.Exit(1);
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