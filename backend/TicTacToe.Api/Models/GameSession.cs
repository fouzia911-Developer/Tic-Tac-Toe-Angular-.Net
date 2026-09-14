namespace TicTacToe.Api.Models;

/// <summary>
/// Server-owned state for a single Tic Tac Toe game. This is the single
/// source of truth referenced in the assignment's "Clarification 1".
/// </summary>
public class GameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // 9 cells, index 0-8, row-major (0,1,2 / 3,4,5 / 6,7,8). null = empty.
    public Player?[] Board { get; set; } = new Player?[9];

    public Player CurrentPlayer { get; set; } = Player.X;

    public GameMode Mode { get; set; } = GameMode.TwoPlayer;

    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public Player? Winner { get; set; }

    public int[]? WinningCells { get; set; }

    public List<MoveRecord> MoveHistory { get; set; } = new();

    public bool HasMoves => MoveHistory.Count > 0;
}
