namespace BLL;

public class GameConfiguration: BaseEntity
{
    public string Name { get; set; } = "Classical 6x6";
    public int BoardWidth { get; set; } = 6;
    public int BoardHeight { get; set; } = 6;
    public int WinCondition { get; set; } = 4;
    public EGameType GameType { get; set; } = EGameType.Rectangle;
    public EAiDifficulty AiDifficulty { get; set; } = EAiDifficulty.Hard;

    public Guid? UserId { get; set; }
}