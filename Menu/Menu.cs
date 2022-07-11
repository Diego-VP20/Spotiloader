using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Spotiloader.Menu;

public class Menu : IMenu
{
    public List<IMenuItem> MenuItems { get; set; } = new();
    private readonly IServiceProvider _serviceProvider;
    public Menu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        LoadOptions();
    }

    public void LoadOptions()
    {
        const string nspace = "Spotiloader.Menu.Options";
        var counter = 1;
        
        var classes = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == nspace && typeof(IMenuItem).IsAssignableFrom(t)
            select t;

        foreach (var menuOption in classes)
        {
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, menuOption);

            if (instance is not IMenuItem menuItem) continue;
            
            menuItem.Id = counter;
            MenuItems.Add(menuItem);
            
            counter++;
        
        }
    }

    public void Show()
    {
        AnsiConsole.Clear();
        
        var counter = 1;
        
        AnsiConsole.MarkupLine("[red bold]0. Exit[/]");

        foreach (var item in MenuItems)
        {
            AnsiConsole.MarkupLine($"[blue]{counter}. {item.Name}[/]");
            counter++;
        }
        
        AnsiConsole.WriteLine();
    }

    public void Execute(IMenuItem item)
    {
        item.Action();
    }

    public void DisableMenuItem(IMenuItem item)
    {
        throw new NotImplementedException();
    }

    public void EnableMenuItem(IMenuItem item)
    {
        throw new NotImplementedException();
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
                return true;
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
        while (true)
        {
            Show();
            
            var unparsedNumber = AnsiConsole.Ask<string>("[orange3 bold]Enter your choice:[/]");

            if (await IsValidChoice(unparsedNumber))
            {
                var number = int.Parse(unparsedNumber);

                if (number == 0)
                {
                    AnsiConsole.Clear();
                    break;
                }
                
                AnsiConsole.Clear();
                
                await MenuItems.Find(i => i.Id == number)!.Action();
                
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[gray]Press any key to continue...[/]");
                Console.ReadKey();
            }
        }
    }
}