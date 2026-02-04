using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Config : BaseEntity
{
    [MaxLength(128, ErrorMessage = "Config Name cannot be longer than 128 characters.")]
    [Required(ErrorMessage = "Config Name is required.")]
    public string ConfigName { get; set; } = default!;

    [Range(3, 20, ErrorMessage = "Board Width must be between 3 and 20.")]
    [Required(ErrorMessage = "Board Width is required.")]
    public int BoardWidth { get; set; }

    [Range(3, 20, ErrorMessage = "Board Height must be between 3 and 20.")]
    [Required(ErrorMessage = "Board Height is required.")]
    public int BoardHeight { get; set; }

    [Range(3, int.MaxValue, ErrorMessage = "Win Condition must be at least 3.")]
    [Required(ErrorMessage = "Win Condition is required.")]
    public int WinCondition { get; set; }

    [Range(0, 1, ErrorMessage = "GameType must between 0 and 1.")]
    [Required(ErrorMessage = "GameType is required.")]
    public int GameType { get; set; } // 0 = Rectangle, 1 = Cylinder

    [Range(0, 1, ErrorMessage = "AiDifficulty must between 0 and 1.")]
    [Required(ErrorMessage = "AiDifficulty is required.")]
    public int AiDifficulty { get; set; } // 0 = Easy, 1 = Hard

    public ICollection<Game>? Games { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string GameTypeDisplayName
    {
        get => GameType == 0 ? "Rectangle" : "Cylinder";
    }

    public string AiDifficultyDisplayName
    {
        get => AiDifficulty == 0 ? "Easy" : "Hard";
    }

    public override string ToString()
    {
        return Id + " " + ConfigName + "(" + BoardWidth + "x" + BoardHeight + ") Games: " +
               (Games?.Count.ToString() ?? "not joined");
    }
}