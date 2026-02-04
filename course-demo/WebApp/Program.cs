using BLL;
using DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
homeDirectory += Path.DirectorySeparatorChar;

connectionString = connectionString.Replace("<db_file>", $"{homeDirectory}app.db");

// AppDbContext is using AddScoped
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// custom dependencies
// AddScoped - instance is created once for web request
// AddSingleton instance is created once
// AddTransient - new instance is created every time
// choose one: either EF based or file system based repo
// builder.Services.AddScoped<IGameRepository<GameConfiguration>, ConfigRepositoryEF>();
// builder.Services.AddScoped<IGameRepository<GameState>, GameRepositoryEF>();
builder.Services.AddScoped<IGameRepository<GameConfiguration>, ConfigRepositoryJson>();
builder.Services.AddScoped<IGameRepository<GameState>, GameRepositoryJson>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(365);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Seed default configurations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Apply migrations
    DefaultConfigurations.SetDefaultConfigs(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

  
app.UseHttpsRedirection();

app.UseRouting();

// Enable session middleware
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();