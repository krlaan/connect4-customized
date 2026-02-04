namespace BLL;

public class GameBrain
{
    private ECellState[,] GameBoard { get; set; }
    private GameConfiguration GameConfiguration { get; set; }
    public string GameName { get; set; } = "Game";
    public string Player1Name { get; set; }
    public string Player2Name { get; set; }
    public EPlayerType Player1Type { get; private set; }
    public EPlayerType Player2Type { get; private set; }
    public string? SaveFileName { get; set; }

    private bool NextMoveByX { get; set; } = true;

    public GameBrain(GameConfiguration configuration, string player1Name, string player2Name,
        EPlayerType player1Type = EPlayerType.Human, EPlayerType player2Type = EPlayerType.Human)
    {
        GameConfiguration = configuration;
        Player1Name = player1Name;
        Player2Name = player2Name;
        Player1Type = player1Type;
        Player2Type = player2Type;
        GameBoard = new ECellState[configuration.BoardWidth, configuration.BoardHeight];
    }

    public bool IsNextPlayerX() => NextMoveByX;

    public ECellState[,] GetBoard()
    {
        var gameBoardCopy = new ECellState[GameConfiguration.BoardWidth, GameConfiguration.BoardHeight];
        Array.Copy(GameBoard, gameBoardCopy, GameBoard.Length);
        return gameBoardCopy;
    }

    public GameState ToGameState()
    {
        var state = new GameState
        {
            GameName = GameName,
            Player1Name = Player1Name,
            Player2Name = Player2Name,
            Player1Type = Player1Type,
            Player2Type = Player2Type,
            Configuration = GameConfiguration,
            NextMoveByX = NextMoveByX,
            UserId = GameConfiguration.UserId,
        };

        state.InitializeBoard();

        for (int y = 0; y < GameConfiguration.BoardHeight; y++)
        for (int x = 0; x < GameConfiguration.BoardWidth; x++)
            state.Board[y][x] = GameBoard[x, y];

        return state;
    }

    public void LoadFromGameState(GameState state)
    {
        GameName = state.GameName;
        NextMoveByX = state.NextMoveByX;
        Player1Type = state.Player1Type;
        Player2Type = state.Player2Type;

        for (int y = 0; y < GameConfiguration.BoardHeight; y++)
        for (int x = 0; x < GameConfiguration.BoardWidth; x++)
            GameBoard[x, y] = state.Board[y][x];
    }

    public (int placedX, int placedY) ProcessMove(int x)
    {
        // validate column
        if (x < 0 || x >= GameConfiguration.BoardWidth) return (-1, -1);

        // find the lowest empty cell in column x (highest y index)
        for (var y = GameConfiguration.BoardHeight - 1; y >= 0; y--)
        {
            if (GameBoard[x, y] == ECellState.Empty)
            {
                GameBoard[x, y] = NextMoveByX ? ECellState.X : ECellState.O;
                NextMoveByX = !NextMoveByX;
                return (x, y);
            }
        }

        // column full
        return (-1, -1);
    }

    private (int dirX, int dirY) GetDirection(int directionIndex) =>
        directionIndex switch
        {
            0 => (-1, -1), // Diagonal up-left
            1 => (0, -1), // Vertical
            2 => (1, -1), // Diagonal up-right
            3 => (1, 0), // Horizontal
            _ => (0, 0)
        };

    private bool BoardCoordinatesAreValid(int x, int y)
    {
        // For cylinder mode, x wraps around; for rectangle mode, x must be in bounds
        if (GameConfiguration.GameType == EGameType.Cylinder)
        {
            return y >= 0 && y < GameConfiguration.BoardHeight;
        }

        return x >= 0 && x < GameConfiguration.BoardWidth && y >= 0 && y < GameConfiguration.BoardHeight;
    }

    public ECellState GetWinner(int x, int y)
    {
        if (GameBoard[x, y] == ECellState.Empty) return ECellState.Empty;

        for (var directionIndex = 0; directionIndex < 4; directionIndex++)
        {
            var (dirX, dirY) = GetDirection(directionIndex);
            var count = CountInDirection(x, y, dirX, dirY);
            count += CountInDirection(x, y, -dirX, -dirY) - 1; // Subtract 1 to avoid counting center cell twice

            if (count >= GameConfiguration.WinCondition)
                return GameBoard[x, y] == ECellState.X ? ECellState.XWin : ECellState.OWin;
        }

        return ECellState.Empty;
    }

    private int CountInDirection(int startX, int startY, int dirX, int dirY)
    {
        var count = 0;
        var x = startX;
        var y = startY;
        var targetCell = GameBoard[startX, startY];

        while (BoardCoordinatesAreValid(x, y) &&
               GameBoard[GetWrappedX(x), y] == targetCell)
        {
            count++;
            x += dirX;
            y += dirY;
        }

        return count;
    }

    private int GetWrappedX(int x)
    {
        if (GameConfiguration.GameType == EGameType.Cylinder)
        {
            return ((x % GameConfiguration.BoardWidth) + GameConfiguration.BoardWidth) % GameConfiguration.BoardWidth;
        }

        return x;
    }

    public GameConfiguration GetConfiguration() => GameConfiguration;
}