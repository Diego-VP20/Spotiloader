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
            var config = await LoadConfigFileAsync();
        }

        private static async Task PresentProgramAsync()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Spotiloader").Centered().Color(Color.Green));
            AnsiConsole.Write(new Markup("[green]V1.0 by Diego-VP20[/]").Centered());
            await Task.Delay(1000);
        }
        
        private static async Task<SpotifyApplication> LoadConfigFileAsync()
        {
            var config = new SpotifyApplication();

            await AnsiConsole.Status()
                .AutoRefresh(true)
                .StartAsync("Checking configuration...", async ctx =>
                {
                    ctx.Spinner(Spinner.Known.Circle);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    await Task.Delay(3000);
                    
                    if (!ConfigManager.IsConfigFile())
                    {
                        ctx.Status("[orange3 bold]First launch detected[/]");
                        await Task.Delay(1000);
                        ctx.Status("[green bold]Creating config file...[/]");
                        await Task.Delay(1000);
                        ctx.Status("[green bold]Done.[/]");

                        await Task.Delay(3000);
                        
                        config = await ConfigManager.GetConfig();
                    }
                    else
                    {
                        ctx.Status("[green bold]Loading Config file...[/]");
                        await Task.Delay(1000);
                        ctx.Status("[green bold]Done.[/]");

                        await Task.Delay(3000);
                        
                        config = await ConfigManager.GetConfig();
                    }
                });

            return config;
        }
    }
}