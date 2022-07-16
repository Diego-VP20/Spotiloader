using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Spotiloader.Menu;

public class Menu : IMenu
{
    public List<IMenuItem> MenuItems { get; set; } = new();
    private readonly IServiceProvider _serviceProvider;
    public bool IsAuthenticated;
    public Menu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void LoadOptions()
    {
        MenuItems.Clear();
        
        const string nspace = "Spotiloader.Menu.Options";
        var counter = 1;
        
        var classes = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == nspace && typeof(IMenuItem).IsAssignableFrom(t) select t;

        foreach (var menuOption in classes)
        {
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, menuOption);

            if (instance is not IMenuItem menuItem) continue;
            
            menuItem.Id = counter;

            menuItem.Enabled = !menuOption.GetCustomAttributes().Any(a => a is RequiresSpotifyAuthentication) || IsAuthenticated;
            
            MenuItems.Add(menuItem);
            
            counter++;
        }
    }

    public void Show()
    {
        AnsiConsole.Clear();

        if (!IsAuthenticated)
        {
            AnsiConsole.MarkupLine("[purple bold](Unauthenticated) You won't be able to use the functions until" +
                                   " you authenticate with Spotify. Please update your credentials![/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green bold](Authenticated) Welcome![/]");
        }
        
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[red bold]0. Exit[/]");

        foreach (var item in MenuItems.Where(item => item.Enabled))
        {
            AnsiConsole.MarkupLine($"[blue]{item.Id}. {item.Name}[/]");
        }
        
        AnsiConsole.WriteLine();
    }

    public async Task Execute(int number)
    {
        await MenuItems.First(i => i.Id == number).Action();
    }

    private async Task<bool> IsValidChoice(string unparsedNumber)
    {
        if (!int.TryParse(unparsedNumber, out var number))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[orange3 bold]Invalid choice[/]");
            await Task.Delay(1000);
            return false;
        }

        switch (number)
        {
            case >= 1 when number <= MenuItems.Count:
                
                if (MenuItems.First(i => i.Id == number).Enabled) return true;
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[orange3 bold]Invalid choice[/]");
                await Task.Delay(1000);
                return false;
            case 0:
                return true;
            default:
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[orange3 bold]Invalid choice[/]");
                await Task.Delay(1000);
                return false;
        }
    }

    public async Task Run()
    {
        LoadOptions();
        
        while (true)
        {
            Show();
            
            var unparsedNumber = AnsiConsole.Ask<string>("[orange3 bold]Enter your choice:[/]");

            if (!await IsValidChoice(unparsedNumber)) continue;
            
            var number = int.Parse(unparsedNumber);

            if (number == 0)
            {
                AnsiConsole.Clear();
                break;
            }
                
            AnsiConsole.Clear();

            await Execute(number);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[gray]Press any key to continue...[/]");
            Console.ReadKey();
        }
    }
}