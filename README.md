# Connect4 Customized

A full-stack implementation of the classic Connect4 game with advanced customization features, AI opponents, and both console and web-based interfaces.

## Overview

This project implements Connect4 with extended and customizable rules, allowing players to:
- Customize board dimensions and winning connection size
- Choose between rectangular or cylindrical board layouts
- Play against AI opponents with adjustable difficulty levels
- Save and continue games across different platforms
- Play in both console and web applications
- Compete with other players in real-time multiplayer mode

## Technology Stack

- **Language**: C# (.NET 6.0+)
- **Console Framework**: .NET Console Application
- **Web Framework**: ASP.NET Core with Razor Pages
- **Database**: Entity Framework Core with SQL Server
- **Data Format**: JSON for file-based storage

## Game Rules & Documentation

For detailed game rules and examples, see:
- [Math is Fun - Connect4 Game](https://www.mathsisfun.com/games/connect-crazy.html)
- [Hasbro Official Instructions](https://instructions.hasbro.com/en-my/instruction/connect-4-game)
- [WikiHow - How to Play Connect4](https://www.wikihow.com/Play-Connect-4)

### Extended Rules

- **Customizable Board**: Configure board width, height, and winning connection length
- **Board Topology**: Choose between standard rectangular or cylindrical (wrapping edges) boards
- **Pre-configured Profiles**: Classical 6x6

## Features

### Game Modes
- **Human vs Human**: Local two-player gameplay
- **Human vs AI**: Play against computer opponents
- **AI vs AI**: Watch AI players compete against each other
- **Difficulty Levels**: Adjustable AI intelligence (Easy and Hard)

### Data Management
- **JSON-based Storage**: Save and load games using JSON text files
- **Entity Framework Database**: Persist games and configurations in SQL database
- **Seamless Switching**: Switch between JSON and database storage with minimal code changes
- **Cross-platform Continuity**: Start a game in console app, continue in web app, and vice versa

### AI Engine
- **Minimax Algorithm**: Intelligent game solving with alpha-beta pruning
- **Dynamic Difficulty**: Performance-scaled AI for various skill levels
- **Quick Analysis**: Optimized for real-time decision making

### Presentation Layers
- **Console Application**: Turn-based gameplay with menu-driven interface
- **Web Application**: ASP.NET Core Razor Pages with multiplayer support
- **Animations**: Visual feedback for game actions in both platforms

## Building & Running

### Prerequisites
- .NET 6.0 or higher
- Visual Studio, Visual Studio Code, or JetBrains Rider

### Console Application
```bash
cd course-demo/ConsoleApp
dotnet run
```

### Web Application
```bash
cd course-demo/WebApp
dotnet run
```

## Configuration

### Switching Between JSON and Database Storage

The application supports both JSON file-based storage and Entity Framework database storage. To switch between them:

#### Console Application
Edit `course-demo/ConsoleApp/Program.cs` and change the `useDatabase` variable:
```csharp
useDatabase: false;  // Set to false for JSON, true for database
```

#### Web Application
Edit `course-demo/WebApp/Program.cs` and swap the service registrations:

**To use Entity Framework (Database):**
```csharp
builder.Services.AddScoped<IGameRepository<GameConfiguration>, ConfigRepositoryEF>();
builder.Services.AddScoped<IGameRepository<GameState>, GameRepositoryEF>();
// builder.Services.AddScoped<IGameRepository<GameConfiguration>, ConfigRepositoryJson>();
// builder.Services.AddScoped<IGameRepository<GameState>, GameRepositoryJson>();
```

**To use JSON (File Storage):**
```csharp
// builder.Services.AddScoped<IGameRepository<GameConfiguration>, ConfigRepositoryEF>();
// builder.Services.AddScoped<IGameRepository<GameState>, GameRepositoryEF>();
builder.Services.AddScoped<IGameRepository<GameConfiguration>, ConfigRepositoryJson>();
builder.Services.AddScoped<IGameRepository<GameState>, GameRepositoryJson>();
```

The repository pattern allows for quick switching with minimal code changes, keeping all game logic independent of the storage mechanism.

## Usage Examples

### Starting a New Game
1. Launch the console application
2. Select or create a game configuration
3. Navigate to "New Game" from the main menu
4. Choose game mode (Human vs Human, Human vs AI, etc.)
5. Start playing

### Saving & Loading Games
- Press "s" to save the game (JSON or Database)
- Access "Load Game" menu to continue previous games
- Seamlessly switch between console and web platforms

### Web Multiplayer
- Open the web application in multiple browsers or tabs
- Create a new game or join an existing one
- Invite players to join via game ID
- Play in real-time with multiple concurrent games
