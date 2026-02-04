using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Game: BaseEntity
{
    [MaxLength(128)]
    public string GameName { get; set; } = "Game";
    
    [MaxLength(128)]
    public string Player1Name { get; set; } = "Player 1";
    
    [MaxLength(128)]
    public string Player2Name { get; set; } = "Player 2";
    
    [MaxLength(10240)]
    public string GameState { get; set; } = default!;
    
    // Expose the Foreign Key
    public Guid ConfigId { get; set; }
    public Config? Config { get; set; }
    
    public Guid? UserId { get; set; }
    public User? User { get; set; }
}