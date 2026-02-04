using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Domain;
using BLL;
using DAL;

namespace WebApp.Pages_Configs
{
    public class CreateModel : PageModel
    {
        private readonly IGameRepository<GameConfiguration> _configRepository;

        public CreateModel(IGameRepository<GameConfiguration> configRepository)
        {
            _configRepository = configRepository;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? UserId { get; set; }

        [BindProperty]
        public Config Config { get; set; } = default!;
        
        public IActionResult OnGet()
        {
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

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
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

            _configRepository.Save(gc);

            return RedirectToPage("./Index", new { userId = userId });
        }
    }
}
