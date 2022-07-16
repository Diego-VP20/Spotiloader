using Spotiloader.API;
using Spotiloader.Config;

namespace Spotiloader.Menu.Options;

public class UpdateCredentialsOption : IMenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "Update Credentials";
    public bool Enabled { get; set; }
    private readonly Menu _menu;
    private readonly SpotifyService _spotifyService;
    
    public UpdateCredentialsOption(Menu menu, SpotifyService spotifyService)
    {
        _menu = menu;
        _spotifyService = spotifyService;
    }
    
    public async Task Action()
    {
        var newConfig = await ConfigManager.InitializeConfigAsync(updatingConfig: true);

        await _spotifyService.Init(newConfig);
        
        _menu.IsAuthenticated = _spotifyService.IsAuthenticated();
        
        _menu.LoadOptions();
    }
}