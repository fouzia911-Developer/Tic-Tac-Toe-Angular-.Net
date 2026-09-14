using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Registered as a singleton so the scoreboard persists across all game
/// sessions for the lifetime of the running backend process (in-memory storage).
/// </summary>
public class ScoreboardService : IScoreboardService
{
    private readonly object _lock = new();
    private readonly Scoreboard _scoreboard = new();

    public Scoreboard Get()
    {
        lock (_lock)
        {
            // Return a copy so callers can't mutate internal state directly.
            return new Scoreboard
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public void RecordXWin()
    {
        lock (_lock) { _scoreboard.XWins++; }
    }

    public void RecordOWin()
    {
        lock (_lock) { _scoreboard.OWins++; }
    }

    public void RecordDraw()
    {
        lock (_lock) { _scoreboard.Draws++; }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _scoreboard.XWins = 0;
            _scoreboard.OWins = 0;
            _scoreboard.Draws = 0;
        }
    }
}
