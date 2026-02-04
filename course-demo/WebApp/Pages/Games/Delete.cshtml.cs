using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL;
using DAL;
using Domain;

namespace WebApp.Pages_Games
{
    public class DeleteModel : PageModel
    {
        private readonly IGameRepository<GameState> _gameRepository;

        public DeleteModel(IGameRepository<GameState> gameRepository)
        {
            _gameRepository = gameRepository;
        }

        [BindProperty]
        public Game Game { get; set; } = default!;
        
        public string GameId { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public Guid? UserId { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Missing id: go back to list instead of 404
                var fallbackUserId = UserId ?? TryGetSessionUserId();
                return RedirectToPage("./Index", new { userId = fallbackUserId });
            }

            // Prefer URL userId over session
            var resolvedUserId = UserId ?? TryGetSessionUserId();
            if (!resolvedUserId.HasValue)
            {
                return RedirectToPage("/Index", new { error = "Please log in first." });
            }
            var userId = resolvedUserId.Value;

            GameId = id;
            
            try
            {
                var gameState = _gameRepository.Load(id);
                if (gameState.UserId.HasValue && gameState.UserId.Value != userId)
                {
                    // Not owner: go back to list
                    return RedirectToPage("./Index", new { userId = userId });
                }

                // Convert to Domain.Game for display
                Game = new Game
                {
                    GameName = $"{gameState.Player1Name} vs {gameState.Player2Name}",
                    Player1Name = gameState.Player1Name,
                    Player2Name = gameState.Player2Name,
                    Config = new Config
                    {
                        ConfigName = $"{gameState.Configuration.BoardWidth}x{gameState.Configuration.BoardHeight}",
                        BoardWidth = gameState.Configuration.BoardWidth,
                        BoardHeight = gameState.Configuration.BoardHeight,
                        GameType = (int) gameState.Configuration.GameType,
                        AiDifficulty = (int) gameState.Configuration.AiDifficulty
                    }
                };

                return Page();
            }
            catch
            {
                // Already deleted or invalid id: go back to list
                return RedirectToPage("./Index", new { userId = userId });
            }
        }

        public async Task<IActionResult> OnPostAsync(string id, Guid? userId)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Missing id: go back to list instead of 404
                return RedirectToPage("./Index", new { userId = userId });
            }

            // Prefer userId from POST or fall back to session
            if (!userId.HasValue)
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToPage("/Index", new { error = "Please log in first." });
                }
                userId = Guid.Parse(userIdString);
            }

            try
            {
                var existing = _gameRepository.Load(id);
                if (existing.UserId.HasValue && existing.UserId.Value != userId)
                {
                    return NotFound();
                }
                _gameRepository.Delete(id);
            }
            catch
            {
                // Ignore if file doesn't exist
            }

            return RedirectToPage("./Index", new { userId = userId });
        }

        private Guid? TryGetSessionUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return null;
            return Guid.Parse(userIdString);
        }
    }
}
