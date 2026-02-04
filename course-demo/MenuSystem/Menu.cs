namespace MenuSystem;

public class Menu
{
    private string Title { get; set; }
    private readonly Dictionary<string, MenuItem> MenuItems = new();
    private readonly Dictionary<string, MenuItem> SystemItems = new();

    private EMenuLevel Level { get; set; }
    
    public Menu(string title, EMenuLevel level)
    {
        Title = title;
        Level = level;
        
        if (level == EMenuLevel.Secondary || level == EMenuLevel.Deep)
        {
            AddSystemItem("b", "Back to Previous Menu");
        }
        if (level == EMenuLevel.Deep)
        {
            AddSystemItem("m", "Return to Main Menu");
        }
        AddSystemItem("e", "Exit");
    }
    
    private void AddSystemItem(string key, string value)
    {
        // Manual check that this is not b, x and m etc...
        if (MenuItems.ContainsKey(key) || SystemItems.ContainsKey(key)) 
            throw new ArgumentException($"Key '{key}' already exists.");
        
        SystemItems[key] = new MenuItem { Key = key, Value = value };
    }
    
    public void AddMenuItem(string key, string value, Func<string> methodToRun)
    {
        if (MenuItems.ContainsKey(key))
        {
            throw new ArgumentException($"Menu item with key '{key}' already exists.");
        }
        
        MenuItems[key] = new MenuItem() {Key = key, Value = value, MethodToRun = methodToRun};
    }
    
    public string Run()
    {
        var menuRunning = true;
        var userChoice = "";
        
        do
        {
            // Clear any previous output (e.g. game board) before showing menu
            Console.Clear();
            DisplayMenu();
            
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            if (input == null)
            {
                Console.WriteLine("Invalid input. Please try again.");
                continue;
            }
            Console.Clear();
            
            userChoice = input.Trim().ToLower();
            
            if (SystemItems.ContainsKey(userChoice))
            {
                menuRunning = false;
            }
            else if (MenuItems.ContainsKey(userChoice))
            {
                var returnValue = MenuItems[userChoice].MethodToRun?.Invoke();

                if (returnValue == "e")
                {
                    menuRunning = false;
                    userChoice = "e";
                }
                else if (returnValue == "m" && Level != EMenuLevel.Main)
                {
                    menuRunning = false;
                    userChoice = "m";
                }
                else if (returnValue == "b" && Level != EMenuLevel.Main)
                {
                    menuRunning = false;
                    userChoice = "b";
                }
            }
            else
            {
                Console.WriteLine("Invalid option. Please try again.");
            }

            Console.WriteLine();
        } while (menuRunning);

        return userChoice; 
    }

    private void DisplayMenu()
    {
        Console.WriteLine(Title);
        Console.WriteLine("────────────────────");
        foreach (var item in MenuItems.Values)
        {
            Console.WriteLine(item);
        }

        foreach (var item in SystemItems.Values)
        {
            Console.WriteLine(item);
        }
    }
}
