using BLL;

namespace ConsoleUI;

public static class Ui
{
    public static void DrawBoard(ECellState[,] gameBoard)
    {
        var boardWidth = gameBoard.GetLength(0);
        var boardHeight = gameBoard.GetLength(1);
        
        for (var x = 0; x < boardWidth; x++)
        {
            Console.Write($"  {x + 1} ");
        }
        Console.WriteLine();
        
        Console.Write("╔");
        for (var x = 0; x < boardWidth; x++)
        {
            if (x < boardWidth - 1) Console.Write("   ╦");
        }
        Console.WriteLine("   ╗");
        
        for (var y = 0; y < boardHeight; y++)
        {
            Console.Write("║");
            for (var x = 0; x < boardWidth; x++)
            {
                DrawCell(gameBoard[x, y]);
                Console.Write("║");
            }

            Console.WriteLine();
            
            if (y < boardHeight - 1)
            {
                Console.Write("╠");
                for (int x = 0; x < boardWidth; x++)
                {
                    Console.Write("═══");
                    if (x < boardWidth - 1) Console.Write("╬");
                }
                Console.WriteLine("╣");
            }
        }
        Console.Write("╚");
        for (int x = 0; x < boardWidth; x++)
        {
            Console.Write("═══");
            if (x < boardWidth - 1) Console.Write("╩");
        }
        Console.WriteLine("╝");
    }
    
    public static void ShowNextPlayer(bool isNextPlayerX, string player1Name, string player2Name)
    {
        var currentPlayerName = isNextPlayerX ? player1Name : player2Name;
        var symbol = isNextPlayerX ? "X" : "O";
    
        Console.WriteLine($"Next Player: {currentPlayerName} ({symbol})");
    }
    
    public static void DropPieceAnimated(ECellState[,] board, int col, ECellState piece)
    {
        var height = board.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            if (board[col, y] != ECellState.Empty)
                break;

            board[col, y] = piece;

            Console.Clear();
            DrawBoard(board);

            Thread.Sleep(70);
            board[col, y] = ECellState.Empty;
        }
    }
    
    private static void DrawCell(ECellState cellValue)
    {
        switch (cellValue)
        {
            case ECellState.Empty:
                Console.Write("   ");
                break;
            case ECellState.X:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(" X ");
                Console.ResetColor();
                break;
            case ECellState.O:
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write(" O ");
                Console.ResetColor();
                break;
            case ECellState.XWin:
            case ECellState.OWin:
            default:
                Console.Write(" ? ");
                break;
        }
    }
}