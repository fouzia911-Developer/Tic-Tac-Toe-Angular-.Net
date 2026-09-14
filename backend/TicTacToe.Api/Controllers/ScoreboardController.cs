using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboardService;

    public ScoreboardController(IScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    /// <summary>Get the session-level scoreboard (X wins / O wins / Draws).</summary>
    [HttpGet]
    public ActionResult<ScoreboardResponse> Get()
    {
        return Ok(ScoreboardResponse.From(_scoreboardService.Get()));
    }

    /// <summary>Reset the scoreboard back to zero.</summary>
    [HttpPost("reset")]
    public ActionResult<ScoreboardResponse> Reset()
    {
        _scoreboardService.Reset();
        return Ok(ScoreboardResponse.From(_scoreboardService.Get()));
    }
}
