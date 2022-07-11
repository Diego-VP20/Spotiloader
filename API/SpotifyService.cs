using Spectre.Console;
using Spotiloader.Config;

namespace Spotiloader.API;

public class SpotifyService
{
    private SpotifyApplication _spotifyApiSettings = null!;
    
    public void Init(SpotifyApplication config)
    {
        _spotifyApiSettings = config;
    }

    public void Test()
    {
        AnsiConsole.MarkupLine($"[darkcyan]{_spotifyApiSettings.ClientId}/{_spotifyApiSettings.ClientSecret}[/]");
    }
}