using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL;
using DAL;
using Domain;

namespace WebApp.Pages_Configs
{
    public class DeleteModel : PageModel
    {
        private readonly IGameRepository<GameConfiguration> _configRepository;

        public DeleteModel(IGameRepository<GameConfiguration> configRepository)
        {
            _configRepository = configRepository;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? UserId { get; set; }

        [BindProperty]
        public Config Config { get; set; } = default!;
        
        public string ConfigId { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            Guid userId;
            if (UserId.HasValue)
            {
                userId = UserId.Value;
            }
            else
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToPage("/Index", new { error = "Please log in first." });
                }
                userId = Guid.Parse(userIdString);
            }

            ConfigId = id;
            
            try
            {
                var gameConfig = _configRepository.Load(id);
                if (gameConfig.UserId.HasValue && gameConfig.UserId.Value != userId)
                {
                    return NotFound();
                }
                
                Config = new Config
                {
                    Id = gameConfig.Id,
                    ConfigName = gameConfig.Name,
                    BoardWidth = gameConfig.BoardWidth,
                    BoardHeight = gameConfig.BoardHeight,
                    WinCondition = gameConfig.WinCondition,
                    GameType = (int) gameConfig.GameType,
                    AiDifficulty = (int) gameConfig.AiDifficulty,
                    UserId = gameConfig.UserId
                };

                return Page();
            }
            catch
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            Guid userId;
            if (UserId.HasValue)
            {
                userId = UserId.Value;
            }
            else
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
                var existing = _configRepository.Load(id);
                if (existing.UserId.HasValue && existing.UserId.Value != userId)
                {
                    return NotFound();
                }
                _configRepository.Delete(id);
            }
            catch
            {
                // Ignore if file doesn't exist
            }

            return RedirectToPage("./Index", new { userId = userId });
        }
    }
}
