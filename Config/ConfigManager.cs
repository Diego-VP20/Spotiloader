using Newtonsoft.Json;
using Spectre.Console;

namespace Spotiloader.Config;

public static class ConfigManager
{
    private static readonly string ConfigFile = AppDomain.CurrentDomain.BaseDirectory + "config.json";

    private static bool IsConfigFile() => File.Exists(ConfigFile);

    private static async Task<SpotifyApplication> CreateRepairOrSaveConfigFile(string clientId = "", string clientSecret = "")
    {
        var config = new SpotifyApplication
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        await File.WriteAllTextAsync(ConfigFile, JsonConvert.SerializeObject(config, Formatting.Indented));

        return config;
    }

    private static async Task<bool> IsValidConfigAsync()
    {
        try
        {
            JsonConvert.DeserializeObject<SpotifyApplication>(await File.ReadAllTextAsync(ConfigFile));
            return true;
        }
        catch (JsonSerializationException)
        {
            return false;
        }
    }

    private static async Task<SpotifyApplication> GetConfigAsync()
    {
        return JsonConvert.DeserializeObject<SpotifyApplication>(await File.ReadAllTextAsync(ConfigFile));
    }

    private static bool ConfigIsEmpty(SpotifyApplication config)
    {
        return string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.ClientSecret);
    }

    public static async Task<SpotifyApplication> InitializeConfigAsync()
    {
        var config = new SpotifyApplication
        {
            ClientId = "",
            ClientSecret = ""
        };
        
        await AnsiConsole.Status()
            .AutoRefresh(true)
            .StartAsync("[green bold]Checking configuration...[/]", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));

                var isConfigFile = IsConfigFile();
                bool isValidConfig;
                
                if (isConfigFile)
                {
                    isValidConfig = await IsValidConfigAsync();    
                }
                else
                {
                    isValidConfig = true;
                }
                
                await Task.Delay(3000);
                    
                if (!isConfigFile)
                {
                    ctx.Status("[orange3 bold]First launch detected[/]");
                    await Task.Delay(1000);
                    ctx.Status("[green bold]Creating config file...[/]");
                    await Task.Delay(1000);
                    await CreateRepairOrSaveConfigFile().ContinueWith(_ =>
                    {
                        ctx.Status("[green bold]Done.[/]");
                        Task.Delay(1000);
                    });
                }

                if (!isValidConfig)
                {
                    ctx.Status("[orange3 bold]Invalid config file, repairing config...[/]");
                    await Task.Delay(1000);

                    await CreateRepairOrSaveConfigFile().ContinueWith(_ =>
                    {
                        ctx.Status("[green bold]Done.[/]");
                        Task.Delay(1000);
                    });
                }

                ctx.Status("[green bold]Loading Config file...[/]");
                await Task.Delay(1000);

                var configTask = GetConfigAsync();
                
                await configTask.ContinueWith(_ =>
                {
                    ctx.Status("[green bold]Done.[/]");
                    Task.Delay(1000);
                });

                config = await configTask;
            });

        if (!ConfigIsEmpty(config)) return config;
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[orange3 bold]Config is currently empty. Please fill in the config file.[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[blue bold]To get the config please visit: https://developer.spotify.com/dashboard/login[/]");
        AnsiConsole.MarkupLine("[blue bold]1. Login[/]");
        AnsiConsole.MarkupLine("[blue bold]2. Create an application[/]");
        AnsiConsole.MarkupLine("[blue bold]3. Copy and paste the Client ID and Client Secret here.[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red bold]Your data will NOT be shared with anyone![/]");
        AnsiConsole.WriteLine();
        
        var clientId = AnsiConsole.Ask<string>("[blue bold]Client ID:[/]");
        var clientSecret = AnsiConsole.Ask<string>("[blue bold]Client Secret:[/]");

        await AnsiConsole.Status()
            .AutoRefresh(true)
            .StartAsync("[green bold]Saving config...[/]", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                ctx.Status("[green bold]Saving config...[/]");
                
                var configTask = CreateRepairOrSaveConfigFile(clientId, clientSecret);

                await Task.Delay(1000);
                
                await configTask.ContinueWith(_ =>
                {
                    ctx.Status("[green bold]Done.[/]");
                });
        
                AnsiConsole.Clear();
                
                return await configTask;
            });
        return config;
    }
}