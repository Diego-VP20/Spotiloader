namespace Spotiloader.Menu;

public interface IMenu
{
    public List<IMenuItem> MenuItems { get; set; }
    public void LoadOptions();
    public void Show();
    public Task Run();
    public void Execute(IMenuItem item);
    public void DisableMenuItem(IMenuItem item);
    public void EnableMenuItem(IMenuItem item);
}