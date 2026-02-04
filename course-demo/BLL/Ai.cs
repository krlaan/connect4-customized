namespace BLL;

public class Ai
{
    private readonly GameConfiguration _config;
    private const int MaxSearchDepth = 6;
    private const int WinScore = 10000;
    private const int LossScore = -10000;
    private const int ThreeInRowScore = 100;
    private const int TwoInRowScore = 10;
    private const int OneInRowScore = 1;
    private readonly Random _random = new();

    public Ai(GameConfiguration config)
    {
        _config = config;
    }

    public int GetBestMove(GameBrain gameBrain)
    {
        var availableMoves = GetAvailableMoves(gameBrain);
        
        if (availableMoves.Count == 0)
            return -1;

        // Easy mode: random move
        if (_config.AiDifficulty == EAiDifficulty.Easy)
        {
            return availableMoves[_random.Next(availableMoves.Count)];
        }

        // Hard mode: minimax
        var bestMove = availableMoves[0];
        var bestScore = int.MinValue;

        foreach (var move in availableMoves)
        {
            var testBrain = CloneGameBrain(gameBrain);
            testBrain.ProcessMove(move);

            // After AI moves, it's human's turn (minimizing)
            var score = Minimax(testBrain, MaxSearchDepth - 1, int.MinValue, int.MaxValue, false);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Minimax(GameBrain gameBrain, int depth, int alpha, int beta, bool isMaximizing)
    {
        // Terminal states - check if anyone won
        var terminalScore = EvaluateTerminal(gameBrain);
        if (terminalScore != 0)
            return terminalScore;

        // Depth limit reached - evaluate position heuristically
        if (depth == 0)
            return EvaluateBoard(gameBrain);

        var availableMoves = GetAvailableMoves(gameBrain);

        // No moves available (board full)
        if (availableMoves.Count == 0)
            return 0;

        if (isMaximizing)
        {
            // AI turn - maximize score
            int maxScore = int.MinValue;

            foreach (var move in availableMoves)
            {
                var testBrain = CloneGameBrain(gameBrain);
                testBrain.ProcessMove(move);

                // After AI moves, it's human's turn
                var score = Minimax(testBrain, depth - 1, alpha, beta, false);
                maxScore = Math.Max(score, maxScore);
                alpha = Math.Max(alpha, score);

                // Alpha-beta pruning
                if (alpha >= beta)
                    break;
            }

            return maxScore;
        }

        // Human turn - minimize score
        var minScore = int.MaxValue;

        foreach (var move in availableMoves)
        {
            var testBrain = CloneGameBrain(gameBrain);
            testBrain.ProcessMove(move);

            // After human moves, it's AI's turn
            var score = Minimax(testBrain, depth - 1, alpha, beta, true);
            minScore = Math.Min(score, minScore);
            beta = Math.Min(beta, score);

            // Alpha-beta pruning
            if (alpha >= beta)
                break;
        }

        return minScore;
    }

    private int EvaluateTerminal(GameBrain gameBrain)
    {
        var board = gameBrain.GetBoard();

        for (var y = 0; y < _config.BoardHeight; y++)
        for (var x = 0; x < _config.BoardWidth; x++)
        {
            if (board[x, y] == ECellState.Empty) continue;

            var winner = gameBrain.GetWinner(x, y);
            if (winner == ECellState.Empty) continue;

            return winner == ECellState.OWin ? WinScore : LossScore;
        }

        return 0;
    }

    private int EvaluateBoard(GameBrain gameBrain)
    {
        var board = gameBrain.GetBoard();
        var score = 0;

        // Iterate all positions and count pieces/threats
        for (var y = 0; y < _config.BoardHeight; y++)
        {
            for (var x = 0; x < _config.BoardWidth; x++)
            {
                if (board[x, y] == ECellState.Empty)
                    continue;

                var isAi = board[x, y] == ECellState.O;
                var pieceScore = isAi ? OneInRowScore : -OneInRowScore;
                score += pieceScore;

                // Evaluate lines starting from this position
                // Horizontal
                score += EvaluateLine(board, x, y, 1, 0, isAi);
                // Vertical
                score += EvaluateLine(board, x, y, 0, 1, isAi);
                // Diagonal down-right
                score += EvaluateLine(board, x, y, 1, 1, isAi);
                // Diagonal down-left
                score += EvaluateLine(board, x, y, -1, 1, isAi);
            }
        }

        return score;
    }

    private int EvaluateLine(ECellState[,] board, int startX, int startY, int dirX, int dirY, bool isAi)
    {
        var targetPiece = isAi ? ECellState.O : ECellState.X;
        var count = 0;
        var score = 0;

        // Count forward from start position
        var x = startX + dirX;
        var y = startY + dirY;

        while (IsValidCoordinate(x, y) && board[GetWrappedX(x), y] == targetPiece)
        {
            count++;
            x += dirX;
            y += dirY;
        }

        // Score based on consecutive count
        if (count >= _config.WinCondition - 1)
        {
            // 3-in-a-row (for WinCondition=4) or more
            score = count >= _config.WinCondition ? (isAi ? ThreeInRowScore : -ThreeInRowScore) : 
                           (isAi ? TwoInRowScore : -TwoInRowScore);
        }
        else if (count > 0)
        {
            score = isAi ? (count * OneInRowScore) : -(count * OneInRowScore);
        }

        return score;
    }
    
    private List<int> GetAvailableMoves(GameBrain gameBrain)
    {
        var moves = new List<int>();
        var board = gameBrain.GetBoard();

        for (var x = 0; x < _config.BoardWidth; x++)
        {
            // Check if top row of this column is empty
            if (board[x, 0] == ECellState.Empty)
            {
                moves.Add(x);
            }
        }

        return moves;
    }

    private GameBrain CloneGameBrain(GameBrain original)
    {
        var clone = new GameBrain(_config, original.Player1Name, original.Player2Name)
        {
            SaveFileName = original.SaveFileName
        };

        // Use GameState for proper cloning
        var state = original.ToGameState();
        clone.LoadFromGameState(state);

        return clone;
    }

    private bool IsValidCoordinate(int x, int y)
    {
        // For cylinder mode, x wraps around; for rectangle mode, x must be in bounds
        if (_config.GameType == EGameType.Cylinder)
        {
            return y >= 0 && y < _config.BoardHeight;
        }
        return x >= 0 && x < _config.BoardWidth && y >= 0 && y < _config.BoardHeight;
    }

    private int GetWrappedX(int x)
    {
        if (_config.GameType == EGameType.Cylinder)
        {
            return ((x % _config.BoardWidth) + _config.BoardWidth) % _config.BoardWidth;
        }
        return x;
    }
}