using System.Text.Json;
using BLL;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class GameRepositoryEF : IGameRepository<GameState>
{
    private readonly AppDbContext _dbContext;

    public GameRepositoryEF(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public List<(string id, string description)> List()
    {
        return _dbContext.GameStates
            .Include(g => g.Config)
            .AsEnumerable()
            .Select(g =>
            {
                var width = g.Config?.BoardWidth ?? 0;
                var height = g.Config?.BoardHeight ?? 0;

                return (
                    Id: g.Id.ToString(),
                    Description: $"{g.Player1Name}_vs_{g.Player2Name}_{width}x{height}"
                );
            })
            .ToList();
    }
    
    public async Task<List<(string id, string description)>> ListAsync()
    {
        return (await _dbContext.GameStates
            .Include(g => g.Config)
            .Select(g => new
            {
                g.Id,
                g.Player1Name,
                g.Player2Name,
                Width = g.Config!.BoardWidth,
                Height = g.Config.BoardHeight
            })
            .ToListAsync())
            .Select(g => 
                (g.Id.ToString(), $"{g.Player1Name}_vs_{g.Player2Name}_{g.Width}x{g.Height}"))
            .ToList();
    }
    
    public async Task<List<Domain.Game>> GetAllGamesForDisplay()
    {
        // AsNoTracking improves performance for read-only operations
        return await _dbContext.GameStates
            .Include(g => g.Config)
            .Include(g => g.User)
            .AsNoTracking()
            .ToListAsync();
    }


    public string Save(GameState data)
    {
        var game = new Domain.Game
        {
            Id = data.Id,
            GameName = data.GameName,
            Player1Name = data.Player1Name,
            Player2Name = data.Player2Name,
            GameState = JsonSerializer.Serialize(data),
            ConfigId = data.Configuration.Id,
            UserId = data.UserId
        };

        _dbContext.GameStates.Add(game);
        _dbContext.SaveChanges();

        return game.Id.ToString();
    }

    public GameState Load(string id)
    {
        var guid = Guid.Parse(id);
        var game = _dbContext.GameStates.Include(g => g.Config).FirstOrDefault(e => e.Id == guid)
                     ?? throw new Exception("Game not found");

        return JsonSerializer.Deserialize<GameState>(game.GameState)
               ?? throw new Exception("Failed to deserialize game state");
    }

    public string Update(GameState data, string id)
    {
        var guid = Guid.Parse(id);
        var game = _dbContext.GameStates.FirstOrDefault(e => e.Id == guid)
                     ?? throw new Exception("Game not found");

        game.GameName = data.GameName;
        game.Player1Name = data.Player1Name;
        game.Player2Name = data.Player2Name;
        game.GameState = JsonSerializer.Serialize(data);
        game.ConfigId = data.Configuration.Id;
        game.UserId = data.UserId ?? game.UserId;

        _dbContext.SaveChanges();
        return id;
    }

    public void Delete(string id)
    {
        var guid = Guid.Parse(id);
        var game = _dbContext.GameStates.FirstOrDefault(e => e.Id == guid);
        if (game == null) return;

        _dbContext.GameStates.Remove(game);
        _dbContext.SaveChanges();
    }
}