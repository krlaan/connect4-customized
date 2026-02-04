using System.Text.Json;
using BLL;

namespace DAL;

public class GameRepositoryJson : IGameRepository<GameState>
{
    public List<(string id, string description)> List()
    {
        var dir = FilesystemHelpers.GetGameDirectory();
        var res = new List<(string id, string description)>();

        foreach (var fullFileName in Directory.EnumerateFiles(dir))
        {  
            var fileName = Path.GetFileName(fullFileName);
            if (!fileName.EndsWith(".json")) continue;
            try
            {
                var jsonText = File.ReadAllText(fullFileName);
                var gameState = JsonSerializer.Deserialize<GameState>(jsonText);
                var desc = (gameState != null && !string.IsNullOrWhiteSpace(gameState.GameName))
                    ? gameState.GameName
                    : Path.GetFileNameWithoutExtension(fileName);
                res.Add((Path.GetFileName(fileName), desc));
            }
            catch
            {
                res.Add((Path.GetFileName(fileName), Path.GetFileNameWithoutExtension(fileName)));
            }
        }

        return res;
    }
    
    public async Task<List<(string id, string description)>> ListAsync()
    {
        return List();
    }
    
    public async Task<List<Domain.Game>> GetAllGamesForDisplay()
    {
        var dir = FilesystemHelpers.GetGameDirectory();
        var res = new List<Domain.Game>();

        foreach (var fullFileName in Directory.EnumerateFiles(dir))
        {  
            var fileName = Path.GetFileName(fullFileName);
            if (!fileName.EndsWith(".json")) continue;
            
            try
            {
                var jsonText = File.ReadAllText(fullFileName);
                var gameState = JsonSerializer.Deserialize<GameState>(jsonText);
                
                if (gameState != null)
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var game = new Domain.Game
                    {
                        Id = Guid.NewGuid(),
                        GameName = $"{gameState.Player1Name} vs {gameState.Player2Name}",
                        Player1Name = gameState.Player1Name,
                        Player2Name = gameState.Player2Name,
                        // Keep filename (without extension) so navigation can load the correct file
                        GameState = fileNameWithoutExt,
                        UserId = gameState.UserId,
                        Config = new Domain.Config
                        {
                            ConfigName = string.IsNullOrWhiteSpace(gameState.Configuration.Name)
                                ? $"{gameState.Configuration.BoardWidth}x{gameState.Configuration.BoardHeight}"
                                : gameState.Configuration.Name,
                            BoardWidth = gameState.Configuration.BoardWidth,
                            BoardHeight = gameState.Configuration.BoardHeight,
                            GameType = (int) gameState.Configuration.GameType,
                            AiDifficulty = (int) gameState.Configuration.AiDifficulty,
                            UserId = gameState.Configuration.UserId
                        }
                    };
                    res.Add(game);
                }
            }
            catch
            {
                // Skip invalid files
            }
        }

        return res;
    }

    public string Save(GameState data)
    {
        var jsonStr = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{data.Player1Name}_vs_{data.Player2Name}" +
                       $"_{data.Configuration.BoardWidth}x{data.Configuration.BoardHeight}_{timestamp}.json";
        var fullFileName = Path.Combine(FilesystemHelpers.GetGameDirectory(), fileName);
        
        File.WriteAllText(fullFileName, jsonStr);
        return fileName;
    }

    public GameState Load(string fileName)
    {
        if (!fileName.EndsWith(".json")) fileName += ".json";
        
        var jsonFileName = Path.Combine(FilesystemHelpers.GetGameDirectory(), fileName);
        var jsonText = File.ReadAllText(jsonFileName);
        var conf = JsonSerializer.Deserialize<GameState>(jsonText);

        return conf ?? throw new NullReferenceException("Json deserialization returned null. Data: " + jsonText);
    }
    
    public string Update(GameState data, string fileName)
    {
        if (!fileName.EndsWith(".json")) fileName += ".json";
        
        var jsonStr = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var fullFileName = Path.Combine(FilesystemHelpers.GetGameDirectory(), fileName);
        File.WriteAllText(fullFileName, jsonStr); 
        return fileName;
    }

    public void Delete(string fileName)
    {
        if (!fileName.EndsWith(".json")) fileName += ".json";
        
        var jsonFileName = Path.Combine(FilesystemHelpers.GetGameDirectory(), fileName);
        
        if (File.Exists(jsonFileName))
        {
            File.Delete(jsonFileName);
        }
    }
}