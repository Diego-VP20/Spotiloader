using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spotiloader.Config;

namespace Spotiloader
{
    public static class Program
    {
        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(l => l.AddSerilog())
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
        
        public static async Task Main()
        {
            // Init logger configuration
            InitLoggerConfiguration();
            
            // Configure services
            await using var serviceProvider = ConfigureServices();
            
            // Present program to user
            await PresentProgramAsync();
            
            // Check for config file
            var config = await ConfigManager.InitializeConfigAsync();
        }

        private static async Task PresentProgramAsync()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Spotiloader").Centered().Color(Color.Green));
            AnsiConsole.Write(new Markup("[green]V1.0 by Diego-VP20[/]").Centered());
            await Task.Delay(1000);
        }
    }
}