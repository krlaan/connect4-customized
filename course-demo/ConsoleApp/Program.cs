using ConsoleApp;

GameMenu.Init(useDatabase: false);

var menu = GameMenu.MainMenu(); // Menu configuration is in GameMenu.cs
menu.Run();

Console.WriteLine("Thanks for Playing!");
