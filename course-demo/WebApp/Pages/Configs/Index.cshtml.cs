using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL;
using DAL;
using Domain;

namespace WebApp.Pages_Configs
{
    public class IndexModel : PageModel
    {
        private readonly IGameRepository<GameConfiguration> _configRepository;

        public IndexModel(IGameRepository<GameConfiguration> configRepository)
        {
            _configRepository = configRepository;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? UserId { get; set; }

        public IList<ConfigRow> Configs { get; set; } = new List<ConfigRow>();

        public class ConfigRow // DTO for beautiful view on page
        {
            public string Id { get; set; } = default!;
            public Config Config { get; set; } = default!;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Prefer userId from URL; fall back to session
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

            List<(string id, string description)> list;
            if (_configRepository is ConfigRepositoryEF efRepo)
            {
                list = await efRepo.GetConfigsByUser(userId); // Filter in EF
            }
            else
            {
                list = await _configRepository.ListAsync(); // Reading Json files
            }

            foreach (var (id, _) in list) // since list is List<(string id, string description)> type
            {
                try
                {
                    var gc = _configRepository.Load(id);
                    if (gc.UserId.HasValue && gc.UserId.Value != userId)
                    {
                        continue;
                    }

                    Configs.Add(new ConfigRow
                    {
                        Id = id,
                        Config =
                        {
                            Id = gc.Id,
                            ConfigName = gc.Name,
                            BoardWidth = gc.BoardWidth,
                            BoardHeight = gc.BoardHeight,
                            WinCondition = gc.WinCondition,
                            GameType = (int) gc.GameType,
                            AiDifficulty = (int) gc.AiDifficulty,
                            UserId = gc.UserId
                        }
                    });
                }
                catch
                {
                    // Skip invalid entries
                }
            }

            return Page();
        }
    }
}