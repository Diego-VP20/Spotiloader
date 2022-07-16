using Spectre.Console;
using Spotiloader.API;

namespace Spotiloader.Menu.Options;

[RequiresSpotifyAuthentication]
public class DownloadSongOption : IMenuItem
{
    public int Id { get; set; }
    
    public bool Enabled { get; set; }
    public string Name { get; set; } = "Download Song";

    private readonly SpotifyService _spotifyService;

    public DownloadSongOption(SpotifyService service)
    {
        _spotifyService = service;
    }
    
    public async Task Action()
    {
        var track = AnsiConsole.Ask<string>("[blue bold]Enter a track url: [/]");
        var name = await _spotifyService.GetTrackNameByUrlAsync(track);
        AnsiConsole.MarkupLineInterpolated($"[blue bold]Song name: {name}[/]");

        if (string.IsNullOrEmpty(name))
        {
            return;            
        }

        await _spotifyService.DownloadTrackByName(name);
    }
}