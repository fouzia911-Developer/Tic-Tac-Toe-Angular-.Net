using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IScoreboardService
{
    Scoreboard Get();
    void RecordXWin();
    void RecordOWin();
    void RecordDraw();
    void Reset();
}
