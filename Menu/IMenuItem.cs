namespace Spotiloader.Menu;

[AttributeUsage(AttributeTargets.Class)]
public class RequiresSpotifyAuthentication : Attribute{}

public interface IMenuItem
{
    /// <summary>
    /// Leave empty, this will be automatically set by the menu.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the menu item that will be displayed.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Sets whether the menu item is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Function that well be run when the menu item is selected.
    /// </summary>
    public Task Action();
}