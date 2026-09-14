using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Owns all game session state in memory (per assignment: in-memory storage is acceptable).
/// Registered as a singleton so state survives across requests for the lifetime of the process.
///
/// Undo policy (see README "Clarification 2"): Option A - Undo is disabled once a game
/// has completed (Won/Draw). The scoreboard is therefore always final for a completed game
/// and never needs to be reversed.
/// </summary>
public class GameService : IGameService
{
    private readonly ConcurrentDictionary<Guid, GameSession> _games = new();
    private readonly IScoreboardService _scoreboard;
    private readonly object _lock = new();

    public GameService(IScoreboardService scoreboard)
    {
        _scoreboard = scoreboard;
    }

    public GameSession CreateGame(GameMode mode)
    {
        var game = new GameSession { Mode = mode };
        _games[game.Id] = game;
        return game;
    }

    public GameSession GetGame(Guid id)
    {
        lock (_lock)
        {
            return GetGameInternal(id);
        }
    }

    public GameSession ApplyMove(Guid id, Player player, int cellIndex)
    {
        lock (_lock)
        {
            var game = GetGameInternal(id);

            if (game.Status != GameStatus.InProgress)
            {
                throw new GameValidationException("This game has already finished. Reset or start a new game to continue playing.");
            }

            if (cellIndex < 0 || cellIndex > 8)
            {
                throw new GameValidationException("Move is outside the board. cellIndex must be between 0 and 8.");
            }

            if (player != game.CurrentPlayer)
            {
                throw new GameValidationException($"It is not {player}'s turn.");
            }

            if (game.Board[cellIndex] is not null)
            {
                throw new GameValidationException("That cell is already occupied.");
            }

            ApplyMoveAndUpdateState(game, player, cellIndex);

            // In Computer mode, once the human has moved (and the game is still in
            // progress) the computer immediately makes its move as part of the same
            // request/response cycle, per the assignment's functional requirements.
            if (game.Mode == GameMode.VsComputer &&
                game.Status == GameStatus.InProgress &&
                game.CurrentPlayer == Player.O)
            {
                var computerCellIndex = GameLogic.GetComputerMove(game.Board);
                ApplyMoveAndUpdateState(game, Player.O, computerCellIndex);
            }

            return game;
        }
    }

    public GameSession Undo(Guid id)
    {
        lock (_lock)
        {
            var game = GetGameInternal(id);

            if (!CanUndo(game))
            {
                throw new GameValidationException("There are no moves available to undo.");
            }

            if (game.Mode == GameMode.TwoPlayer)
            {
                // Two Player Mode: remove only the single most recent move.
                RemoveLastMove(game);
            }
            else
            {
                // Computer Mode: remove the computer's last move together with the
                // human move that preceded it, so control returns to the human (X).
                RemoveLastMove(game);
                if (game.MoveHistory.Count > 0)
                {
                    RemoveLastMove(game);
                }
            }

            return game;
        }
    }

    public GameSession Reset(Guid id)
    {
        lock (_lock)
        {
            var existing = GetGameInternal(id);
            // New session state, same id and mode; scoreboard is untouched.
            var fresh = new GameSession { Id = existing.Id, Mode = existing.Mode };
            _games[existing.Id] = fresh;
            return fresh;
        }
    }

    public bool CanUndo(GameSession game) =>
        game.Status == GameStatus.InProgress && game.HasMoves;

    // ---------- private helpers ----------

    private GameSession GetGameInternal(Guid id)
    {
        if (!_games.TryGetValue(id, out var game))
        {
            throw new GameNotFoundException(id);
        }
        return game;
    }

    private void ApplyMoveAndUpdateState(GameSession game, Player player, int cellIndex)
    {
        game.Board[cellIndex] = player;
        game.MoveHistory.Add(new MoveRecord
        {
            MoveNumber = game.MoveHistory.Count + 1,
            Player = player,
            CellIndex = cellIndex
        });

        var winningLine = GameLogic.FindWinningLine(game.Board, player);
        if (winningLine is not null)
        {
            game.Status = GameStatus.Won;
            game.Winner = player;
            game.WinningCells = winningLine;

            // Scoreboard updates exactly once per completed game.
            if (player == Player.X) _scoreboard.RecordXWin();
            else _scoreboard.RecordOWin();
        }
        else if (GameLogic.IsBoardFull(game.Board))
        {
            game.Status = GameStatus.Draw;
            _scoreboard.RecordDraw();
        }
        else
        {
            game.CurrentPlayer = player == Player.X ? Player.O : Player.X;
        }
    }

    private static void RemoveLastMove(GameSession game)
    {
        var last = game.MoveHistory[^1];
        game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
        game.Board[last.CellIndex] = null;
        game.CurrentPlayer = last.Player; // it's that player's turn again
        game.Status = GameStatus.InProgress;
        game.Winner = null;
        game.WinningCells = null;
    }
}
