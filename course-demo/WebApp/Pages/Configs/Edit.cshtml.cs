using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;
using BLL;

namespace WebApp.Pages_Configs
{
    public class EditModel : PageModel
    {
        private readonly IGameRepository<GameConfiguration> _configRepository;

        public EditModel(IGameRepository<GameConfiguration> configRepository)
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
            if (string.IsNullOrEmpty(id)) return NotFound();

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
                var gc = _configRepository.Load(id);
                if (gc.UserId.HasValue && gc.UserId.Value != userId)
                {
                    return NotFound();
                }
                Config = MapToDomain(gc);
            }
            catch
            {
                return NotFound();
            }

            ViewData["GameTypeList"] = new SelectList(new[]
            {
                new { Value = 0, Text = "Rectangle" },
                new { Value = 1, Text = "Cylinder" }
            }, "Value", "Text"); // this for beautiful <select> field
            
            ViewData["AiDifficultyList"] = new SelectList(new[]
            {
                new { Value = 0, Text = "Easy" },
                new { Value = 1, Text = "Hard" }
            }, "Value", "Text"); // this for beautiful <select> field
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(id)) return Page();

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

            var gc = new GameConfiguration
            {
                Id = Config.Id,
                Name = Config.ConfigName,
                BoardWidth = Config.BoardWidth,
                BoardHeight = Config.BoardHeight,
                WinCondition = Config.WinCondition,
                GameType = (EGameType) Config.GameType,
                AiDifficulty = (EAiDifficulty) Config.AiDifficulty,
                UserId = userId
            };

            try
            {
                _configRepository.Update(gc, id);
            }
            catch
            {
                return NotFound();
            }

            return RedirectToPage("./Index", new { userId = userId });
        }

        private static Config MapToDomain(GameConfiguration gc) => new Config
        {
            Id = gc.Id,
            ConfigName = gc.Name,
            BoardWidth = gc.BoardWidth,
            BoardHeight = gc.BoardHeight,
            WinCondition = gc.WinCondition,
            GameType = (int) gc.GameType,
            AiDifficulty = (int) gc.AiDifficulty,
            UserId = gc.UserId
        };
    }
}
