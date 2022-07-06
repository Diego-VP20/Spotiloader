using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

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

        private static void InitLoggerConfigurationAsync()
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
            InitLoggerConfigurationAsync();
            
            // Configure services
            await using var serviceProvider = ConfigureServices();
            
            // Present program to user
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Spotiloader").Centered().Color(Color.Green));
            AnsiConsole.Write(new FigletText("V1.0 by Diego-VP20").Centered().Color(Color.Green));
        }
    }
}