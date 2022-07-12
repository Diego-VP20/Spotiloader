using Spectre.Console;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
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
    private EmbedIOAuthServer _server = null!;
    private SpotifyClient? _client;
    
    public async Task Init(SpotifyApplication config)
    {
        _spotifyApiSettings = config;
        
        await StartLoginProcess();
    }

    private async Task StartLoginProcess()
    {
        _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
        await _server.Start();

        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        _server.ErrorReceived += OnErrorReceived;

        var request = new LoginRequest(_server.BaseUri, _spotifyApiSettings.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = new List<string> { Scopes.UserReadEmail }
        };
        
        BrowserUtil.Open(request.ToUri());
    }
    
    private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        await _server.Stop();

        var config = SpotifyClientConfig.CreateDefault();
        var tokenResponse = await new OAuthClient(config).RequestToken(
            new AuthorizationCodeTokenRequest(
                _spotifyApiSettings.ClientId, _spotifyApiSettings.ClientSecret,
                response.Code, new Uri("http://localhost:5000/callback")
            )
        );

        _client = new SpotifyClient(tokenResponse.AccessToken);
    }

    private async Task OnErrorReceived(object sender, string error, string? state)
    {
        AnsiConsole.MarkupLine($"[red bold]Error whilst logging in: {error}[/]");
        await _server.Stop();
    }

    private LinkType GetLinkType(string link, out string? id)
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