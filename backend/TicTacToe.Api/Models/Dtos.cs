namespace TicTacToe.Api.Models;

// ---------- Requests ----------

public class CreateGameRequest
{
    /// <summary>Defaults to TwoPlayer if omitted.</summary>
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}

public class MoveRequest
{
    public Player Player { get; set; }

    /// <summary>0-8 cell index. Row/Column are derived from this on the server.</summary>
    public int CellIndex { get; set; }
}

// ---------- Responses ----------

public class ScoreboardResponse
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }

    public static ScoreboardResponse From(Scoreboard s) => new()
    {
        XWins = s.XWins,
        OWins = s.OWins,
        Draws = s.Draws
    };
}

public class MoveHistoryDto
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int CellIndex { get; set; }
    public int Row { get; set; }      // 1-based for display
    public int Column { get; set; }   // 1-based for display

    public static MoveHistoryDto From(MoveRecord m) => new()
    {
        MoveNumber = m.MoveNumber,
        Player = m.Player,
        CellIndex = m.CellIndex,
        Row = m.Row + 1,
        Column = m.Column + 1
    };
}

public class GameStateResponse
{
    public Guid GameId { get; set; }
    public string?[] Board { get; set; } = new string?[9];
    public Player CurrentPlayer { get; set; }
    public GameMode Mode { get; set; }
    public GameStatus Status { get; set; }
    public Player? Winner { get; set; }
    public int[]? WinningCells { get; set; }
    public List<MoveHistoryDto> MoveHistory { get; set; } = new();
    public bool CanUndo { get; set; }
    public ScoreboardResponse Scoreboard { get; set; } = new();

    public static GameStateResponse From(GameSession game, Scoreboard scoreboard, bool canUndo) => new()
    {
        GameId = game.Id,
        Board = game.Board.Select(c => c?.ToString()).ToArray(),
        CurrentPlayer = game.CurrentPlayer,
        Mode = game.Mode,
        Status = game.Status,
        Winner = game.Winner,
        WinningCells = game.WinningCells,
        MoveHistory = game.MoveHistory.Select(MoveHistoryDto.From).ToList(),
        CanUndo = canUndo,
        Scoreboard = ScoreboardResponse.From(scoreboard)
    };
}

/// <summary>Simple error payload for validation failures (400s).</summary>
public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
}
