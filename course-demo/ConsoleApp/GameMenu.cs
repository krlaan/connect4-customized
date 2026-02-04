using BLL;
using DAL;
using Microsoft.EntityFrameworkCore;


namespace ConsoleApp;

using MenuSystem;

public static class GameMenu
{
    private static IGameRepository<GameState> _gameRepository = default!;
    private static IGameRepository<GameConfiguration> _configRepository = default!;
    private static string _player1Name = "Player 1";
    private static string _player2Name = "Player 2";

    public static void Init(bool useDatabase)
    {
        if (useDatabase)
        {
            var db = GetDbContext();
            _gameRepository = new GameRepositoryEF(db);
            _configRepository = new ConfigRepositoryEF(db);
        }
        else
        {
            _gameRepository = new GameRepositoryJson();
            _configRepository = new ConfigRepositoryJson();
        }
    }

    public static Menu MainMenu()
    {
        var mainMenu = new Menu("Connect4 Main Menu", EMenuLevel.Main);

        mainMenu.AddMenuItem("1", "New Game", () =>
        {
            var chosenConfig = ChooseConfiguration();
            if (chosenConfig == null)
            {
                // user went back or exited from choose menu; don't start a game
                return "";
            }

            while (true)
            {
                var gameMode = ChooseGameMode();
                if (gameMode == null)
                {
                    // user pressed Back in game mode: go one step back to config selection
                    chosenConfig = ChooseConfiguration();
                    if (chosenConfig == null) return "";
                    continue;
                }

                var (player1Type, player2Type) = gameMode.Value;

                var controller = new GameController(chosenConfig, _player1Name, _player2Name,
                    player1Type, player2Type, _gameRepository);

                controller.GameLoop();

                break;
            }

            return "";
        });

        mainMenu.AddMenuItem("2", "Load Game", () => { return LoadGamesMenu(); });

        mainMenu.AddMenuItem("3", "Settings", () =>
        {
            var settingsMenu = new Menu("Settings", EMenuLevel.Secondary);

            settingsMenu.AddMenuItem("1", "Create new config", () => CreateNewConfig());

            // List existing configurations
            var configs = _configRepository.List();
            for (var i = 0; i < configs.Count; i++)
            {
                var index = i;
                var key = (i + 2).ToString();
                var confDesc = configs[i].description;
                settingsMenu.AddMenuItem(key, $"Change configurations: {confDesc}",
                    () =>
                    {
                        var rv = EditConfiguration(configs[index].id);
                        if (rv == "e" || rv == "m") return rv; // allow exit/main to propagate
                        return ""; // treat Back from deep as staying in Settings
                    });
            }

            var res = settingsMenu.Run();
            if (res == "e") return "e";
            return "";
        });

        return mainMenu;
    }

    private static string CreateNewConfig()
    {
        var newConfig = new GameConfiguration();
        var id = _configRepository.Save(newConfig);

        EditConfiguration(id);

        return "";
    }

    private static string EditConfiguration(string id)
    {
        var config = _configRepository.Load(id);
        var currentId = id; // Track current filename in case it changes

        var editMenu = new Menu($"Edit configuration", EMenuLevel.Deep);

        editMenu.AddMenuItem("1", "Change Title", () =>
        {
            Console.WriteLine($"Current name: {config.Name}");
            Console.Write("Enter new name: ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) config.Name = name;
            currentId = _configRepository.Update(config, currentId); // Update currentId if filename changed
            Console.WriteLine("Title updated.");
            Console.ReadLine();
            return "";
        });

        editMenu.AddMenuItem("2", "Change board size", () =>
        {
            ChangeBoardSize(config);
            currentId = _configRepository.Update(config, currentId);
            return "";
        });

        editMenu.AddMenuItem("3", "Change win condition", () =>
        {
            ChangeWinCondition(config);
            currentId = _configRepository.Update(config, currentId);
            return "";
        });

        editMenu.AddMenuItem("4", "Change game type", () =>
        {
            ChangeGameType(config);
            currentId = _configRepository.Update(config, currentId);
            return "";
        });

        editMenu.AddMenuItem("5", "Change AI difficulty", () =>
        {
            ChangeAiDifficulty(config);
            currentId = _configRepository.Update(config, currentId);
            return "";
        });

        editMenu.AddMenuItem("6", "Change player names", () =>
        {
            ChangePlayerNames();
            currentId = _configRepository.Update(config, currentId);
            return "";
        });

        editMenu.AddMenuItem("7", "Delete", () =>
        {
            _configRepository.Delete(currentId);
            Console.WriteLine("Configuration deleted.");
            Console.ReadLine();
            return "m";
        });

        var res = editMenu.Run();
        return res;
    }

    private static string LoadGamesMenu()
    {
        var games = _gameRepository.List();
        if (games.Count == 0)
        {
            Console.WriteLine("No saved games.");
            Console.ReadLine();
            return "";
        }

        var selectMenu = new Menu("Saved Games", EMenuLevel.Secondary);

        for (var i = 0; i < games.Count; i++)
        {
            var fileName = games[i].id;
            var displayName = games[i].description;

            selectMenu.AddMenuItem((i + 1).ToString(), displayName, () =>
            {
                var rv = ManageGame(fileName);
                if (rv == "e" || rv == "m") return rv;
                return "";
            });
        }

        var result = selectMenu.Run();
        return result;
    }

    private static string ManageGame(string fileName)
    {
        var loadedState = _gameRepository.Load(fileName);

        var menuTitle = string.IsNullOrWhiteSpace(loadedState.GameName)
            ? $"{loadedState.Player1Name} vs {loadedState.Player2Name} ({loadedState.Configuration.BoardWidth}x{loadedState.Configuration.BoardHeight})"
            : loadedState.GameName;

        var menu = new Menu(menuTitle, EMenuLevel.Deep);

        menu.AddMenuItem("1", "Continue Game", () =>
        {
            var gameBrain = new GameBrain(
                loadedState.Configuration,
                loadedState.Player1Name,
                loadedState.Player2Name,
                loadedState.Player1Type,
                loadedState.Player2Type
            );
            gameBrain.LoadFromGameState(loadedState);

            gameBrain.SaveFileName = fileName;

            var controller = new GameController(gameBrain, _gameRepository);
            controller.GameLoop();
            return "m";
        });

        menu.AddMenuItem("2", "Delete Game", () =>
        {
            _gameRepository.Delete(fileName);
            Console.WriteLine("Game deleted! Press Enter to continue...");
            Console.ReadLine();
            return "m";
        });

        return menu.Run();
    }

    private static GameConfiguration? ChooseConfiguration()
    {
        var configs = _configRepository.List();

        if (configs.Count == 0)
        {
            Console.WriteLine("No saved configurations. Using default configuration.");
            Console.ReadLine();
            return new GameConfiguration();
        }

        var chooseMenu = new Menu("Choose Configuration", EMenuLevel.Secondary);
        GameConfiguration selectedConfig = null!;

        for (var i = 0; i < configs.Count; i++)
        {
            var id = configs[i].id;
            var description = configs[i].description;

            chooseMenu.AddMenuItem((i + 1).ToString(), description, () =>
            {
                selectedConfig = _configRepository.Load(id);
                return "e";
            });
        }

        var result = chooseMenu.Run();

        if (result == "b") return null;
        if (result == "e" && selectedConfig == null) Environment.Exit(0);

        return selectedConfig;
    }

    private static (EPlayerType, EPlayerType)? ChooseGameMode()
    {
        var modeMenu = new Menu("Choose Game Mode", EMenuLevel.Deep);
        (EPlayerType, EPlayerType)? selectedMode = null;

        modeMenu.AddMenuItem("1", "Human vs Human", () =>
        {
            selectedMode = (EPlayerType.Human, EPlayerType.Human);
            return "e";
        });

        modeMenu.AddMenuItem("2", "Human vs AI", () =>
        {
            selectedMode = (EPlayerType.Human, EPlayerType.Ai);
            return "e";
        });

        modeMenu.AddMenuItem("3", "AI vs AI", () =>
        {
            selectedMode = (EPlayerType.Ai, EPlayerType.Ai);
            return "e";
        });

        var result = modeMenu.Run();

        if (result == "b") return null;
        if (result == "e" && selectedMode == null) Environment.Exit(0);

        return selectedMode;
    }

    private static void ChangeBoardSize(GameConfiguration config)
    {
        Console.WriteLine($"Current size: {config.BoardWidth}x{config.BoardHeight}");
        Console.Write("Enter board width: ");
        if (int.TryParse(Console.ReadLine(), out var width))
        {
            config.BoardWidth = width;
        }

        Console.Write("Enter board height: ");
        if (int.TryParse(Console.ReadLine(), out var height))
        {
            config.BoardHeight = height;
        }

        Console.WriteLine($"Board size updated: {config.BoardWidth}x{config.BoardHeight}");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ChangeWinCondition(GameConfiguration config)
    {
        Console.WriteLine($"Current win condition: {config.WinCondition}");
        Console.Write("Enter win condition: ");
        if (int.TryParse(Console.ReadLine(), out var win))
        {
            config.WinCondition = win;
        }

        Console.WriteLine($"Win condition updated: {config.WinCondition}");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ChangeGameType(GameConfiguration config)
    {
        Console.WriteLine($"Current game type: {config.GameType}");
        Console.WriteLine("Enter game type:");
        Console.WriteLine("1) Rectangle (standard)");
        Console.WriteLine("2) Cylinder");
        Console.Write("Your choice (1 or 2): ");
        
        var choice = Console.ReadLine();
        if (choice == "1")
        {
            config.GameType = EGameType.Rectangle;
        }
        else if (choice == "2")
        {
            config.GameType = EGameType.Cylinder;
        }
        else
        {
            Console.WriteLine("Invalid choice. Game type unchanged.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Game type updated: {config.GameType}");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ChangeAiDifficulty(GameConfiguration config)
    {
        Console.WriteLine($"Current AI difficulty: {config.AiDifficulty}");
        Console.WriteLine("Choose AI difficulty:");
        Console.WriteLine("1) Easy");
        Console.WriteLine("2) Hard");
        Console.Write("Your choice (1 or 2): ");
        
        var choice = Console.ReadLine();
        if (choice == "1")
        {
            config.AiDifficulty = EAiDifficulty.Easy;
        }
        else if (choice == "2")
        {
            config.AiDifficulty = EAiDifficulty.Hard;
        }
        else
        {
            Console.WriteLine("Invalid choice. AI difficulty unchanged.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"AI difficulty updated: {config.AiDifficulty}");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static string ChangePlayerNames()
    {
        Console.WriteLine($"Current Player 1: {_player1Name}");
        Console.Write("Enter new name for Player 1: ");
        var newP1 = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newP1))
            _player1Name = newP1;

        Console.WriteLine($"Current Player 2: {_player2Name}");
        Console.Write("Enter new name for Player 2: ");
        var newP2 = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newP2))
            _player2Name = newP2;

        Console.WriteLine($"Names updated! Player 1: {_player1Name}, Player 2: {_player2Name}");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();

        return "";
    }


    static AppDbContext GetDbContext()
    {
        // ========================= DB STUFF ========================
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        homeDirectory = homeDirectory + Path.DirectorySeparatorChar;

        // We are using SQLite
        var connectionString = $"Data Source={homeDirectory}app.db";

        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            // .LogTo(Console.WriteLine)
            .Options;

        var dbContext = new AppDbContext(contextOptions);

        // apply any pending migrations (recreates db as needed)
        dbContext.Database.Migrate();

        return dbContext;
    }
}