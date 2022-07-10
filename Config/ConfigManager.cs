using Newtonsoft.Json;
using Spectre.Console;

namespace Spotiloader.Config;

public static class ConfigManager
{
    private static readonly string ConfigFile = AppDomain.CurrentDomain.BaseDirectory + "config.json";

    private static bool IsConfigFile() => File.Exists(ConfigFile);

    private static async Task CreateOrRepairConfigFile()
    {
        var config = new SpotifyApplication
        {
            ClientId = "",
            ClientSecret = ""
        };
        
        await File.WriteAllTextAsync(ConfigFile, JsonConvert.SerializeObject(config, Formatting.Indented));
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

    public static async Task<SpotifyApplication> InitializeConfigAsync()
    {
        var config = new SpotifyApplication
        {
            ClientId = "",
            ClientSecret = ""
        };
        
        await AnsiConsole.Status()
            .AutoRefresh(true)
            .StartAsync("Checking configuration...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Circle);
                ctx.SpinnerStyle(Style.Parse("green"));

                var isConfigFile = IsConfigFile();
                var isValidConfig = await IsValidConfigAsync();
                    
                await Task.Delay(3000);
                    
                if (!isConfigFile)
                {
                    ctx.Status("[orange3 bold]First launch detected[/]");
                    await Task.Delay(1000);
                    ctx.Status("[green bold]Creating config file...[/]");
                    await Task.Delay(1000);
                    await CreateOrRepairConfigFile().ContinueWith(_ =>
                    {
                        ctx.Status("[green bold]Done.[/]");
                        Task.Delay(1000);
                    });
                }

                if (!isValidConfig)
                {
                    ctx.Status("[orange3 bold]Invalid config file, repairing config...[/]");
                    await Task.Delay(1000);

                    await CreateOrRepairConfigFile().ContinueWith(_ =>
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
        
        return config;
    }
}