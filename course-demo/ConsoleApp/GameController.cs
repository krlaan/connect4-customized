using BLL;
using ConsoleUI;
using DAL;

namespace ConsoleApp;

public class GameController
{
    private GameBrain GameBrain { get; set; }
    private readonly IGameRepository<GameState> _gameRepository;
    private readonly Ai? _ai;

    // create new game
    public GameController(GameConfiguration gameConfig, string player1, string player2, 
        EPlayerType player1Type, EPlayerType player2Type, IGameRepository<GameState> repository)
    {
        GameBrain = new GameBrain(gameConfig, player1, player2, player1Type, player2Type);
        _gameRepository = repository;
        
        // Initialize AI if needed
        if (player1Type == EPlayerType.Ai || player2Type == EPlayerType.Ai)
        {
            _ai = new Ai(gameConfig);
        }
    }

    // continue game
    public GameController(GameBrain gameBrain, IGameRepository<GameState> repository)
    {
        GameBrain = gameBrain;
        _gameRepository = repository;

        if (gameBrain.Player1Type == EPlayerType.Ai || gameBrain.Player2Type == EPlayerType.Ai)
        {
            _ai = new Ai(gameBrain.GetConfiguration());
        }
    }

    public void GameLoop()
    {
        var gameOver = false;
        do
        {
            Console.Clear();

            // Draw gameBoard
            Ui.DrawBoard(GameBrain.GetBoard());
            Ui.ShowNextPlayer(GameBrain.IsNextPlayerX(), GameBrain.Player1Name, GameBrain.Player2Name);
            
            var isCurrentPlayerAi = (GameBrain.IsNextPlayerX() && GameBrain.Player1Type == EPlayerType.Ai) ||
                                    (!GameBrain.IsNextPlayerX() && GameBrain.Player2Type == EPlayerType.Ai);

            int col;

            if (isCurrentPlayerAi && _ai != null)
            {
                col = _ai.GetBestMove(GameBrain);
                
                if (col == -1)
                {
                    Console.WriteLine("No valid moves available!");
                    Console.ReadLine();
                    break;
                }

                Console.Write("Press enter for AI move...");
                Console.ReadLine();
            }
            else
            {
                // Human makes move
                Console.Write("Choose column (1-" + GameBrain.GetBoard().GetLength(0) + "):");
                
                var input = Console.ReadLine();
                if (input?.ToLower() == "e")
                {
                    gameOver = true;
                    continue;
                }
                if (input?.ToLower() == "s")
                {
                    SaveGame();
                    gameOver = true;
                    continue;
                }

                if (input == null) continue;
                if (!int.TryParse(input, out col))
                {
                    Console.WriteLine("Invalid input! Press Enter to continue...");
                    Console.ReadLine();
                    continue;
                }

                if (col < 1 || col > GameBrain.GetBoard().GetLength(0))
                {
                    Console.WriteLine("Invalid input! Press Enter to continue...");
                    Console.ReadLine();
                    continue;
                }

                col--; // Convert to 0-based index
            }

            var piece = GameBrain.IsNextPlayerX() ? ECellState.X : ECellState.O;
            
            Ui.DropPieceAnimated(GameBrain.GetBoard(), col, piece);
            
            var (placedX, placedY) = GameBrain.ProcessMove(col);
            if (placedX == -1)
            {
                Console.WriteLine("Invalid or full column. Press Enter to continue...");
                Console.ReadLine();
                continue;
            }

            // redraw board so the last placed piece is visible
            Console.Clear();
            Ui.DrawBoard(GameBrain.GetBoard());

            var winner = GameBrain.GetWinner(placedX, placedY);
            if (winner == ECellState.Empty) continue;
            
            Console.WriteLine("Winner is: " + (winner == ECellState.XWin ? 
                GameBrain.Player1Name : GameBrain.Player2Name));
                
            while (true)
            {
                Console.WriteLine("Save Game or Exit...");
                var choice = Console.ReadLine()?.ToLower();

                if (choice == "s")
                {
                    SaveGame();
                    break;
                }
                if (choice == "e") break;
            }

            break;
        } while (gameOver == false);
    }
    
    private void SaveGame()
    {
        var state = GameBrain.ToGameState();
        state.UserId = null; // Console games are public

        if (string.IsNullOrEmpty(GameBrain.SaveFileName))
        {
            var id = _gameRepository.Save(state);
            GameBrain.SaveFileName = id;
            Console.WriteLine("Game saved!");
        }
        else
        {
            _gameRepository.Update(state, GameBrain.SaveFileName);
            Console.WriteLine("Game updated!");
        }
        
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
}