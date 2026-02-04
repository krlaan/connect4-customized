using BLL;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class ConfigRepositoryEF: IGameRepository<GameConfiguration>
{
    private readonly AppDbContext _dbContext;

    public ConfigRepositoryEF(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public List<(string id, string description)> List()
    {
        return _dbContext.GameConfigurations
            .Select(c => new
            {
                Id = c.Id.ToString(), 
                Description = c.ConfigName
            })
            .AsEnumerable()
            .Select(x => (x.Id, x.Description))
            .ToList();
    }
    
    public async Task<List<(string id, string description)>> ListAsync()
    {
        return (await _dbContext.GameConfigurations
            .Select(c => new
            {
                Id = c.Id.ToString(), 
                Description = c.ConfigName
            })
            .ToListAsync())
            .Select(x => (x.Id, x.Description))
            .ToList();
    }
    
    public async Task<List<(string id, string description)>> GetConfigsByUser(Guid userId)
    {
        // Show both user-owned and global configs
        return (await _dbContext.GameConfigurations
            .Where(c => c.UserId == userId || c.UserId == null)
            .Select(c => new
            {
                Id = c.Id.ToString(), 
                Description = c.ConfigName
            })
            .ToListAsync())
            .Select(x => (x.Id, x.Description))
            .ToList();
    }

    public string Save(GameConfiguration data)
    {
        var config = new Domain.Config
        {
            Id = data.Id,
            ConfigName = data.Name,
            BoardWidth = data.BoardWidth,
            BoardHeight = data.BoardHeight,
            WinCondition = data.WinCondition,
            GameType = (int) data.GameType,
            AiDifficulty = (int) data.AiDifficulty,
            UserId = data.UserId
        };
        _dbContext.GameConfigurations.Add(config);
        _dbContext.SaveChanges();
        return config.Id.ToString();
    }

    public GameConfiguration Load(string id)
    {
        var guid = Guid.Parse(id);
        var config = _dbContext.GameConfigurations.FirstOrDefault(c => c.Id == guid)
               ?? throw new Exception("Configuration not found");
        
        return new GameConfiguration
        {
            Id = config.Id,
            Name = config.ConfigName,
            BoardWidth = config.BoardWidth,
            BoardHeight = config.BoardHeight,
            WinCondition = config.WinCondition,
            GameType = (EGameType) config.GameType,
            AiDifficulty = (EAiDifficulty) config.AiDifficulty,
            UserId = config.UserId
        };
    }

    public string Update(GameConfiguration data, string fileName)
    {
        var guid = Guid.Parse(fileName);
        var existing = _dbContext.GameConfigurations.FirstOrDefault(c => c.Id == guid);
        if (existing == null) throw new Exception("Configuration not found");

        existing.ConfigName = data.Name;
        existing.BoardWidth = data.BoardWidth;
        existing.BoardHeight = data.BoardHeight;
        existing.WinCondition = data.WinCondition;
        existing.GameType = (int) data.GameType;
        existing.AiDifficulty = (int) data.AiDifficulty;
        existing.UserId = data.UserId;

        _dbContext.SaveChanges();
        return fileName;
    }

    public void Delete(string id)
    {
        var guid = Guid.Parse(id);
        var entity = _dbContext.GameConfigurations
            .Include(c => c.Games)
            .FirstOrDefault(c => c.Id == guid);
        
        if (entity == null) return;

        // Manually remove dependent games to satisfy FK restrictions
        if (entity.Games?.Count > 0)
        {
            _dbContext.GameStates.RemoveRange(entity.Games);
        }

        _dbContext.GameConfigurations.Remove(entity);
        _dbContext.SaveChanges();
    }
}