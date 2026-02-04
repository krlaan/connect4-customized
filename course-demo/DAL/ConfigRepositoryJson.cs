using System.Text.Json;
using BLL;

namespace DAL;

public class ConfigRepositoryJson : IGameRepository<GameConfiguration>
{
    public List<(string id, string description)> List()
    {
        var dir = FilesystemHelpers.GetConfigDirectory();
        var res = new List<(string id, string description)>();

        foreach (var fullFileName in Directory.EnumerateFiles(dir))
        {  
            var fileName = Path.GetFileName(fullFileName);
            if (!fileName.EndsWith(".json")) continue;
            
            try
            {
                var config = Load(fileName);
                res.Add((fileName, config.Name));
            }
            catch
            {
                res.Add((fileName, Path.GetFileNameWithoutExtension(fileName)));
            }
        }

        return res;
    }
    
    public async Task<List<(string id, string description)>> ListAsync()
    {
        return List();
    }

    public string Save(GameConfiguration data)
    {
        var jsonStr = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        var fileName = $"{data.Name}_{data.BoardWidth}x{data.BoardHeight}_win={data.WinCondition}.json";
        var fullFileName = Path.Combine(FilesystemHelpers.GetConfigDirectory(), fileName);
        
        File.WriteAllText(fullFileName, jsonStr);
        return fileName;
    }

    public GameConfiguration Load(string id)
    {
        var jsonFileName = Path.Combine(FilesystemHelpers.GetConfigDirectory(), id);
        var jsonText = File.ReadAllText(jsonFileName);
        var conf = JsonSerializer.Deserialize<GameConfiguration>(jsonText);

        return conf ?? throw new NullReferenceException("Json deserialization returned null. Data: " + jsonText);
    }
    
    public string Update(GameConfiguration data, string fileName)
    {
        var jsonStr = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var oldFullFileName = Path.Combine(FilesystemHelpers.GetConfigDirectory(), fileName);
        
        // Generate new filename based on current config data
        var newFileName = $"{data.Name}_{data.BoardWidth}x{data.BoardHeight}_win={data.WinCondition}.json";
        var newFullFileName = Path.Combine(FilesystemHelpers.GetConfigDirectory(), newFileName);
        
        // If filename changed (name was updated), delete old file
        if (oldFullFileName != newFullFileName && File.Exists(oldFullFileName))
        {
            File.Delete(oldFullFileName);
        }
        
        File.WriteAllText(newFullFileName, jsonStr); 
        return newFileName;
    }
    
    public void Delete(string id)
    {
        var jsonFileName = Path.Combine(FilesystemHelpers.GetConfigDirectory(), id);
        
        if (File.Exists(jsonFileName))
        {
            File.Delete(jsonFileName);
        }
    }
}