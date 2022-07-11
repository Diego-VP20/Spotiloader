using Spotiloader.API;

namespace Spotiloader.Menu.Options;

public class DownloadSongOption : IMenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "Download Song";

    private readonly SpotifyService _spotifyService;

    public DownloadSongOption(SpotifyService service)
    {
        _spotifyService = service;
    }
    
    public async Task Action()
    {
        _spotifyService.Test();
        await Task.Delay(1000);
    }
}