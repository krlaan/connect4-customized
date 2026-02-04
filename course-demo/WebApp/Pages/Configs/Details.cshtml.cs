using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;
using BLL;
using DAL;

namespace WebApp.Pages_Configs
{
    public class DetailsModel : PageModel
    {
        private readonly IGameRepository<GameConfiguration> _configRepository;

        public DetailsModel(IGameRepository<GameConfiguration> configRepository)
        {
            _configRepository = configRepository;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? UserId { get; set; }

        public Config Config { get; set; } = default!;
        public string ConfigId { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            ConfigId = id;

            try
            {
                var gc = _configRepository.Load(id);
                Config = MapToDomain(gc);
                return Page();
            }
            catch
            {
                return NotFound();
            }
        }

        private static Config MapToDomain(GameConfiguration gc) => new Config
        {
            Id = gc.Id,
            ConfigName = gc.Name,
            BoardWidth = gc.BoardWidth,
            BoardHeight = gc.BoardHeight,
            WinCondition = gc.WinCondition,
            GameType = (int) gc.GameType,
            AiDifficulty = (int) gc.AiDifficulty
        };
    }
}
