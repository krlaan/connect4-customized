namespace BLL;


public class GameState: BaseEntity
{
    public ECellState[][] Board { get; set; } = [];
    public bool NextMoveByX { get; set; }
    public GameConfiguration Configuration { get; set; } = new GameConfiguration();

    public Guid? UserId { get; set; }
    public bool IsMultiPlayer { get; set; }
    
    public string GameName { get; set; } = "Game";
    public string Player1Name { get; set; } = "Player 1";
    public string Player2Name { get; set; } = "Player 2";
    public EPlayerType Player1Type { get; set; } = EPlayerType.Human;
    public EPlayerType Player2Type { get; set; } = EPlayerType.Human;

    public void InitializeBoard()
    {
        Board = new ECellState[Configuration.BoardHeight][];
        for (int i = 0; i < Configuration.BoardHeight; i++)
            Board[i] = new ECellState[Configuration.BoardWidth];
    }
}