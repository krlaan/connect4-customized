using System.ComponentModel.DataAnnotations;
using BLL;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Pages;

public class NewGame : PageModel
{
    private readonly IGameRepository<GameConfiguration> _configRepository;
    private readonly IGameRepository<GameState> _gameRepository;
    private readonly AppDbContext _context;
    
    public NewGame(IGameRepository<GameConfiguration> configRepository, IGameRepository<GameState> gameRepository, AppDbContext context)
    {
        _configRepository = configRepository;
        _gameRepository = gameRepository;
        _context = context;
    }

    public SelectList ConfigurationSelectList { get; set; } = default!; // Elements for dropdown <select> field
    public string CurrentUserName { get; set; } = "Player 1";
    
    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }
    
    [BindProperty]
    [Length(3,128)]
    public string GameName { get; set; } = default!;
    
    [BindProperty]
    public string ConfigId { get; set; } = default!;
    
    [BindProperty]
    [Required]
    public string GameMode { get; set; } = "pvp";
    
    public async Task OnGetAsync()
    {
        // Prefer URL userId over Session
        if (!UserId.HasValue)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdString))
            {
                UserId = Guid.Parse(userIdString);
            }
        }
        
        // Load UserName from database by UserId
        if (UserId.HasValue)
        {
            var user = await _context.Users.FindAsync(UserId.Value);
            CurrentUserName = user?.UserName ?? "Player 1";
        }
        else
        {
            CurrentUserName = "Player 1";
        }
        
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        List<(string id, string description)> data;
        
        if (UserId.HasValue && _configRepository is ConfigRepositoryEF configRepo)
        {
            data = await configRepo.GetConfigsByUser(UserId.Value);
        }
        else if (UserId.HasValue)
        {
            data = await _configRepository.ListAsync();
            var currentUser = UserId.Value;
            data = data
                .Where(item =>
                {
                    try
                    {
                        var cfg = _configRepository.Load(item.id);
                        return cfg.UserId.HasValue && cfg.UserId.Value == currentUser;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToList();
        }
        else
        {
            data = new List<(string id, string description)>();
        }
        
        var data2 = data.Select(i => new
            {
                id = i.id,
                value = i.description
            }
        ).ToList();
        
        ConfigurationSelectList = new SelectList(data2, "id", "value");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        // Prefer URL userId over Session
        if (!UserId.HasValue)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdString))
            {
                UserId = Guid.Parse(userIdString);
            }
        }
        
        if (!UserId.HasValue)
        {
            return RedirectToPage("./Index", new { error = "Please log in first." });
        }
        
        var userId = UserId.Value;
        
        // Load UserName from database by UserId
        var user = await _context.Users.FindAsync(userId);
        CurrentUserName = user?.UserName ?? "Player 1";
        
        var conf = _configRepository.Load(ConfigId);
        
        var (player1Type, player2Type) = GetPlayerTypeFromGameMode();

        var gameBrain = new GameBrain(conf, CurrentUserName, "Player 2", player1Type, player2Type);
        
        var state = gameBrain.ToGameState();
        state.GameName = GameName;
        state.UserId = userId;
        state.IsMultiPlayer = GameMode == "multiplayer";;
        
        var gameId = _gameRepository.Save(state);

        return RedirectToPage("./GamePlay", new { gameId = gameId, userId = userId });
    }

    private (EPlayerType, EPlayerType) GetPlayerTypeFromGameMode()
    {
        return GameMode switch
        {
            "pvp" => (EPlayerType.Human, EPlayerType.Human),
            "pvai" => (EPlayerType.Human, EPlayerType.Ai),
            "aivai" => (EPlayerType.Ai, EPlayerType.Ai),
            "multiplayer" => (EPlayerType.Human, EPlayerType.Human),
            _ => (EPlayerType.Human, EPlayerType.Human)
        };
    }
}