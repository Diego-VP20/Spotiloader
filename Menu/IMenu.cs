namespace Spotiloader.Menu;

public interface IMenu
{
    public List<IMenuItem> MenuItems { get; set; }
    public void LoadOptions();
    public void Show();
    public Task Run();
    public Task Execute(int number);
}