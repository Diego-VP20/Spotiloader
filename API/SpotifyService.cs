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
    private YoutubeService _youtubeService;

    public SpotifyService(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
    }
    
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
            _client = null;
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

    public async Task<string?> GetTrackNameByUrlAsync(string url)
    {
        if (GetLinkType(url, out var id) == LinkType.Track)
        {
            var track = await _client!.Tracks.Get(id!);
            return track.Name + " " + track.Artists.First().Name;
        }

        AnsiConsole.MarkupLine("[orange3] Invalid link type[/]");
        return null;
    }

    public async Task DownloadTrackByName(string name)
    {
        var result = await _youtubeService.DownloadSongByName(name);

        switch (result)
        {
            case DownloadStatus.Downloaded:
                AnsiConsole.MarkupLine("[green bold]Downloaded successfully[/]");
                AnsiConsole.MarkupLine($"[green bold]File path: [/]{_youtubeService.GetDownloadPath}");
                break;
            case DownloadStatus.NotFound:
                AnsiConsole.MarkupLine("[orange3 bold]No results found[/]");
                break;
            case DownloadStatus.Failed:
                AnsiConsole.MarkupLine("[red bold]Error downloading Song[/]");
                break;
            default:
                throw new ArgumentOutOfRangeException(result.ToString());
        }
    }
}