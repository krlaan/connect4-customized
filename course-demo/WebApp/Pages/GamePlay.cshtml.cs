using BLL;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class GamePlay : PageModel
{
    private readonly IGameRepository<GameState> _gameRepository;
    private Ai? _ai;

    public GamePlay(IGameRepository<GameState> gameRepo)
    {
        _gameRepository = gameRepo;
    }

    public string GameId { get; set; } = default!;
    public GameBrain GameBrain { get; set; } = default!;
    public string GameName { get; set; } = default!;
    public ECellState Winner { get; set; }
    public string WinnerMessage { get; set; } = default!;
    public bool IsAiVsAi { get; set; }
    public bool IsAiTurn { get; set; }
    public bool IsMultiPlayer { get; set; }
    public int CurrentMoveX { get; set; } = -1;
    public int CurrentMoveY { get; set; } = -1;
    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? Seat { get; set; } // "p1" or "p2"
    public bool CanMove { get; set; }

    public async Task<IActionResult> OnGetAsync(string gameId, int? x)
    {
        GameId = gameId;

        var state = _gameRepository.Load(gameId);
        GameBrain = new GameBrain(state.Configuration, state.Player1Name, state.Player2Name, state.Player1Type, state.Player2Type);
        GameBrain.LoadFromGameState(state);
        GameName = state.GameName;
        
        // Restore last move coordinates from TempData for animation
        if (TempData["LastMoveX"] != null && TempData["LastMoveY"] != null)
        {
            CurrentMoveX = (int) TempData["LastMoveX"]!;
            CurrentMoveY = (int) TempData["LastMoveY"]!;
        }
        
        Winner = CheckBoardForWinner();
        if (Winner != ECellState.Empty)
        {
            SetWinnerMessage();
        }
        
        IsAiVsAi = state.Player1Type == EPlayerType.Ai && state.Player2Type == EPlayerType.Ai;
        
        // Initialize AI if needed
        if (state.Player1Type == EPlayerType.Ai || state.Player2Type == EPlayerType.Ai)
        {
            _ai = new Ai(state.Configuration);
        }
        
        if (x.HasValue)
        {
            // Check if current player is AI - then it's an AI move request
            var isCurrentPlayerAi = IsCurrentPlayerAi(state);
            
            if (isCurrentPlayerAi && _ai != null)
            {
                await ProcessAiMove();
            }
            else
            {
                ProcessHumanMove(state, x.Value);
            }
            
            // Update state after move
            var updatedState = GameBrain.ToGameState();
            updatedState.IsMultiPlayer = state.IsMultiPlayer;
            _gameRepository.Update(updatedState, gameId);
            
            // Redirect to remove x from URL (PRG pattern) to prevent duplicate moves on refresh
            return RedirectToPage("/GamePlay", BuildRouteValues(gameId));
        }
        
        IsAiTurn = Winner == ECellState.Empty && IsCurrentPlayerAi(state);

        IsMultiPlayer = state.IsMultiPlayer;

        // Compute if this window can move next
        var seat = (Seat ?? string.Empty).ToLowerInvariant();
        CanMove = ComputeCanMove(state, seat);
        
        return Page();
    }
    
    private void SetWinnerMessage()
    {
        WinnerMessage = Winner == ECellState.XWin 
            ? $"🎉 {GameBrain.Player1Name} wins!" 
            : $"🎉 {GameBrain.Player2Name} wins!";
    }
    
    private ECellState CheckBoardForWinner()
    {
        var board = GameBrain.GetBoard();
        for (var x = 0; x < board.GetLength(0); x++)
        {
            for (var y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] != ECellState.Empty)
                {
                    var winner = GameBrain.GetWinner(x, y);
                    if (winner != ECellState.Empty)
                    {
                        return winner;
                    }
                }
            }
        }
        return ECellState.Empty;
    }
    
    public IActionResult OnPostDeleteFinishedGame(string gameId, string redirectTo, Guid? userId)
    {
        _gameRepository.Delete(gameId);
        
        if (redirectTo == "newgame")
        {
            return RedirectToPage("/NewGame");
        }
        
        return RedirectToPage("/Games/Index", new { userId = userId });
    }
    
    private bool IsCurrentPlayerAi(GameState state) =>
        (GameBrain.IsNextPlayerX() && state.Player1Type == EPlayerType.Ai) ||
        (!GameBrain.IsNextPlayerX() && state.Player2Type == EPlayerType.Ai);

    private async Task ProcessAiMove()
    {
        await Task.Delay(800);
        
        var aiMove = _ai!.GetBestMove(GameBrain);
        if (aiMove >= 0)
        {
            var (px, py) = GameBrain.ProcessMove(aiMove);
            if (px >= 0 && py >= 0)
            {
                StoreMoveForAnimation(px, py);
                Winner = GameBrain.GetWinner(px, py);
                if (Winner != ECellState.Empty)
                    SetWinnerMessage();
            }
        }
    }

    private void ProcessHumanMove(GameState state, int column)
    {
        var seat = (Seat ?? string.Empty).ToLowerInvariant();
        if (CanPlayerMove(state, seat))
        {
            var (px, py) = GameBrain.ProcessMove(column);
            if (px >= 0 && py >= 0)
            {
                StoreMoveForAnimation(px, py);
                Winner = GameBrain.GetWinner(px, py);
                if (Winner != ECellState.Empty)
                    SetWinnerMessage();
            }
        }
    }
    
    private bool CanPlayerMove(GameState state, string seat)
    {
        if (!state.IsMultiPlayer) return true;
        
        var isP1Seat = seat == "p1";
        var isP2Seat = seat == "p2";
        var isP1Turn = GameBrain.IsNextPlayerX();
        
        return (isP1Seat && isP1Turn && state.Player1Type == EPlayerType.Human) ||
               (isP2Seat && !isP1Turn && state.Player2Type == EPlayerType.Human);
    }
    
    private void StoreMoveForAnimation(int x, int y)
    {
        CurrentMoveX = x;
        CurrentMoveY = y;
        TempData["LastMoveX"] = x;
        TempData["LastMoveY"] = y;
    }
    
    private bool ComputeCanMove(GameState state, string seat)
    {
        if (Winner != ECellState.Empty) return false;
        if (!state.IsMultiPlayer) return !IsAiTurn;
        
        var isP1Seat = seat == "p1";
        var isP2Seat = seat == "p2";
        var isP1Turn = GameBrain.IsNextPlayerX();
        
        return (isP1Seat && isP1Turn && state.Player1Type == EPlayerType.Human) ||
               (isP2Seat && !isP1Turn && state.Player2Type == EPlayerType.Human);
    }
    
    private object BuildRouteValues(string gameId)
    {
        if (!string.IsNullOrEmpty(Seat) && UserId.HasValue)
            return new { gameId, userId = UserId.Value, seat = Seat };
        if (!string.IsNullOrEmpty(Seat))
            return new { gameId, seat = Seat };
        if (UserId.HasValue)
            return new { gameId, userId = UserId.Value };
        return new { gameId };
    }
}