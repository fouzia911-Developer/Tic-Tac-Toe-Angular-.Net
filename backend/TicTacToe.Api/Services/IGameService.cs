using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameValidationException : Exception
{
    public GameValidationException(string message) : base(message) { }
}

public class GameNotFoundException : Exception
{
    public GameNotFoundException(Guid id) : base($"Game '{id}' was not found.") { }
}

public interface IGameService
{
    GameSession CreateGame(GameMode mode);
    GameSession GetGame(Guid id);
    GameSession ApplyMove(Guid id, Player player, int cellIndex);
    GameSession Undo(Guid id);
    GameSession Reset(Guid id);
    bool CanUndo(GameSession game);
}
