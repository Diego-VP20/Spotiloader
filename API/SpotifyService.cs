using Spectre.Console;
using SpotifyAPI.Web;
using Spotiloader.Config;

namespace Spotiloader.API;

public enum LinkType
{
    Track,
    Playlist,
    Artist,
    Invalid
}

public class SpotifyService
{
    private SpotifyApplication _spotifyApiSettings = null!;
    private SpotifyClient? _client;
    
    public async Task Init(SpotifyApplication config)
    {
        _spotifyApiSettings = config;

        await StartLoginProcess();
    }

    private async Task StartLoginProcess()
    {
        try
        {
            var config = SpotifyClientConfig.CreateDefault();

            var request = new ClientCredentialsRequest(_spotifyApiSettings.ClientId, _spotifyApiSettings.ClientSecret);
            var response = await new OAuthClient(config).RequestToken(request);

            _client = new SpotifyClient(config.WithToken(response.AccessToken));
        }
        catch (APIException)
        {
            
        }
    }

    private static LinkType GetLinkType(string link, out string? id)
    {
        if (link.StartsWith("https://open.spotify.com/track/"))
        {
            id = link.Split('/').Last().Split("?").First();
            return LinkType.Track;
        }

        if (link.StartsWith("https://open.spotify.com/playlist/"))
        {
            id = link.Split('/').Last().Split("?").First();
            return LinkType.Playlist;
        }

        if (link.StartsWith("https://open.spotify.com/artist/"))
        {
            id = link.Split('/').Last().Split("?").First();
            return LinkType.Artist;
        }

        id = null;
        return LinkType.Invalid;
    }
    
    public bool IsAuthenticated() => _client != null;

    public async void Test(string url)
    {
        if (GetLinkType(url, out var id) == LinkType.Track)
        {
            if (_client == null) return;
            
            var track = await _client.Tracks.Get(id!);
            AnsiConsole.MarkupLine($"[darkcyan bold]Track: {track.Name}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[orange3] Invalid link type[/]");
        }
    }
}