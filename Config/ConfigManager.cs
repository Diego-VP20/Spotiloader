using System.Text.Json;

namespace Spotiloader.Config;

public static class ConfigManager
{
    private static readonly string ConfigFile = AppDomain.CurrentDomain.BaseDirectory + "config.json";

    public static bool IsConfigFile()
    {
        return File.Exists(ConfigFile);
    }

    private static async Task<bool> IsValidConfig()
    {
        try
        {
            await JsonSerializer.DeserializeAsync<SpotifyApplication>(File.OpenRead(ConfigFile));
            return true;
        }catch(JsonException)
        {
            return false;
        }
    }

    public static async Task<SpotifyApplication> GetConfig()
    {
        if (IsConfigFile() && await IsValidConfig()) return (await JsonSerializer.DeserializeAsync<SpotifyApplication>(File.OpenRead(ConfigFile)))!;
    
        var config = new SpotifyApplication
        {
            ClientId = "",
            ClientSecret = ""
        };
    
        await using var file = File.Create(ConfigFile);
        await JsonSerializer.SerializeAsync(file, config);
        await file.DisposeAsync();
    
        File.SetAttributes(ConfigFile, File.GetAttributes(ConfigFile) | FileAttributes.Hidden);
    
        return (await JsonSerializer.DeserializeAsync<SpotifyApplication>(File.OpenRead(ConfigFile)))!;
    }
}