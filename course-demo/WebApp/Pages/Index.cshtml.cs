using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _dbContext;

    [BindProperty]
    public string? UserName { get; set; }

    [BindProperty (SupportsGet = true)]
    public string? Error { get; set; }
    

    public IndexModel(ILogger<IndexModel> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        UserName = UserName?.Trim();
        if (!string.IsNullOrWhiteSpace(UserName))
        {
            // Find or create user
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == UserName);
            if (user == null)
            {
                user = new User { UserName = UserName };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }
            
            HttpContext.Session.SetString("UserName", UserName);
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            return RedirectToPage("/Games/Index", new { userId = user.Id });
        }

        Error = "Please enter a username.";
        return Page();
    }
}