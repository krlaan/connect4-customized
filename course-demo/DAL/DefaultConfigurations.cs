namespace DAL;

public static class DefaultConfigurations
{
    public static void SetDefaultConfigs(AppDbContext context)
    {
        if (context.GameConfigurations.Any(c => 
            c.ConfigName == "Classical 6x6" && 
            c.UserId == null))
        {
            return;
        }

        context.GameConfigurations.Add(new Domain.Config
        {
            ConfigName = "Classical 6x6",
            BoardWidth = 6,
            BoardHeight = 6,
            WinCondition = 4
        });
        context.SaveChanges();
    }
}
