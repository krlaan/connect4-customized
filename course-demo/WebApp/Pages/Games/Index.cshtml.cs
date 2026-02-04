using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL;
using DAL;
using Domain;
using System.Text.Json;

namespace WebApp.Pages_Games
{
    public class IndexModel : PageModel
    {
        private readonly IGameRepository<GameState> _gameRepository;

        public IndexModel(IGameRepository<GameState> gameRepository)
        {
            _gameRepository = gameRepository;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? UserId { get; set; }

        public IList<Game> Game { get;set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Prefer userId from URL; fall back to session for backward compatibility
            if (!UserId.HasValue)
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToPage("/Index", new { error = "Please log in first." });
                }
                UserId = Guid.Parse(userIdString);
            }
            var userId = UserId.Value;

            // Use concrete repository types to access GetAllGamesForDisplay
            if (_gameRepository is GameRepositoryEF efRepo)
            {
                var games = await efRepo.GetAllGamesForDisplay();
                Game = games.Where(g => IsVisibleToUser(g, userId)).ToList();
            }
            else if (_gameRepository is GameRepositoryJson jsonRepo)
            {
                var games = await jsonRepo.GetAllGamesForDisplay();
                Game = games.Where(g => IsVisibleToUser(g, userId)).ToList();
            }
            else
            {
                Game = new List<Game>();
            }

            return Page();
        }

        private static bool IsVisibleToUser(Game game, Guid userId)
        {
            // Owned by current user
            if (game.UserId == userId) return true;

            // Treat console games as public
            if (game.UserId == null) return true;

            // Otherwise, only show multiplayer games
            try
            {
                var json = !string.IsNullOrEmpty(game.GameState) && game.GameState.TrimStart().StartsWith("{");
                if (json)
                {
                    var state = JsonSerializer.Deserialize<GameState>(game.GameState);
                    return state?.IsMultiPlayer ?? false;
                }

                // If GameState is a filename (JSON storage), try to read from disk
                var fileName = game.GameState.EndsWith(".json") ? game.GameState : game.GameState + ".json";
                var fullPath = Path.Combine(DAL.FilesystemHelpers.GetGameDirectory(), fileName);
                if (!System.IO.File.Exists(fullPath)) return false;
                var jsonText = System.IO.File.ReadAllText(fullPath);
                var stateFromFile = JsonSerializer.Deserialize<GameState>(jsonText);
                return stateFromFile?.IsMultiPlayer ?? false;
            }
            catch
            {
                return false;
            }
        }
    }
}
