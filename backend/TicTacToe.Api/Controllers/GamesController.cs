using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IScoreboardService _scoreboardService;

    public GamesController(IGameService gameService, IScoreboardService scoreboardService)
    {
        _gameService = gameService;
        _scoreboardService = scoreboardService;
    }

    /// <summary>Create a new game session.</summary>
    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = _gameService.CreateGame(request.Mode);
        return Ok(ToResponse(game));
    }

    /// <summary>Get the current state of a game session.</summary>
    [HttpGet("{id:guid}")]
    public ActionResult<GameStateResponse> GetGame(Guid id)
    {
        try
        {
            var game = _gameService.GetGame(id);
            return Ok(ToResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>Submit a player move.</summary>
    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateResponse> SubmitMove(Guid id, [FromBody] MoveRequest request)
    {
        try
        {
            var game = _gameService.ApplyMove(id, request.Player, request.CellIndex);
            return Ok(ToResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
        catch (GameValidationException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>Undo the last move (or move-pair in Computer Mode).</summary>
    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateResponse> Undo(Guid id)
    {
        try
        {
            var game = _gameService.Undo(id);
            return Ok(ToResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
        catch (GameValidationException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>Reset the current game (board/history/status only - scoreboard is untouched).</summary>
    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateResponse> Reset(Guid id)
    {
        try
        {
            var game = _gameService.Reset(id);
            return Ok(ToResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
    }

    private GameStateResponse ToResponse(GameSession game) =>
        GameStateResponse.From(game, _scoreboardService.Get(), _gameService.CanUndo(game));
}
