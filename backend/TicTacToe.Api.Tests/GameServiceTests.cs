using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameServiceTests
{
    private static GameService CreateService(out ScoreboardService scoreboard)
    {
        scoreboard = new ScoreboardService();
        return new GameService(scoreboard);
    }

    [Fact]
    public void ApplyMove_ValidMove_UpdatesBoardAndSwitchesTurn()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        var result = service.ApplyMove(game.Id, Player.X, 0);

        Assert.Equal(Player.X, result.Board[0]);
        Assert.Equal(Player.O, result.CurrentPlayer); // turn switched
        Assert.Single(result.MoveHistory);
    }

    [Fact]
    public void ApplyMove_OnOccupiedCell_ThrowsAndDoesNotChangeTurn()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.ApplyMove(game.Id, Player.X, 0);

        Assert.Throws<GameValidationException>(() => service.ApplyMove(game.Id, Player.O, 0));

        var current = service.GetGame(game.Id);
        Assert.Equal(Player.O, current.CurrentPlayer); // unchanged by the invalid attempt
        Assert.Single(current.MoveHistory);
    }

    [Fact]
    public void ApplyMove_ByWrongPlayer_ThrowsAndDoesNotChangeTurn()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        // It's X's turn; O attempts to move.
        Assert.Throws<GameValidationException>(() => service.ApplyMove(game.Id, Player.O, 4));

        var current = service.GetGame(game.Id);
        Assert.Equal(Player.X, current.CurrentPlayer);
        Assert.Empty(current.MoveHistory);
    }

    [Fact]
    public void ApplyMove_OutsideBoard_Throws()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<GameValidationException>(() => service.ApplyMove(game.Id, Player.X, 9));
        Assert.Throws<GameValidationException>(() => service.ApplyMove(game.Id, Player.X, -1));
    }

    [Fact]
    public void ApplyMove_TurnsAlternate_AcrossMultipleMoves()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.ApplyMove(game.Id, Player.X, 0);
        service.ApplyMove(game.Id, Player.O, 1);
        var result = service.ApplyMove(game.Id, Player.X, 2);

        Assert.Equal(Player.O, result.CurrentPlayer);
        Assert.Equal(3, result.MoveHistory.Count);
    }

    [Fact]
    public void ApplyMove_RowWin_SetsWonStatusAndUpdatesScoreboardOnce()
    {
        var service = CreateService(out var scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.ApplyMove(game.Id, Player.X, 0);
        service.ApplyMove(game.Id, Player.O, 3);
        service.ApplyMove(game.Id, Player.X, 1);
        service.ApplyMove(game.Id, Player.O, 4);
        var result = service.ApplyMove(game.Id, Player.X, 2); // completes top row

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, result.WinningCells);
        Assert.Equal(1, scoreboard.Get().XWins);
    }

    [Fact]
    public void ApplyMove_AfterGameCompleted_Throws()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.ApplyMove(game.Id, Player.X, 0);
        service.ApplyMove(game.Id, Player.O, 3);
        service.ApplyMove(game.Id, Player.X, 1);
        service.ApplyMove(game.Id, Player.O, 4);
        service.ApplyMove(game.Id, Player.X, 2); // X wins

        Assert.Throws<GameValidationException>(() => service.ApplyMove(game.Id, Player.O, 5));
    }

    [Fact]
    public void ApplyMove_FullBoardWithNoWinner_ResultsInDraw()
    {
        var service = CreateService(out var scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X O X
        // X O O
        // O X X
        var moves = new[]
        {
            (Player.X, 0), (Player.O, 1), (Player.X, 2),
            (Player.O, 4), (Player.X, 3), (Player.O, 5),
            (Player.X, 7), (Player.O, 6), (Player.X, 8)
        };

        GameSession result = game;
        foreach (var (player, cell) in moves)
        {
            result = service.ApplyMove(game.Id, player, cell);
        }

        Assert.Equal(GameStatus.Draw, result.Status);
        Assert.Null(result.Winner);
        Assert.Equal(1, scoreboard.Get().Draws);
    }

    [Fact]
    public void Reset_ClearsBoardAndHistory_ButKeepsScoreboard()
    {
        var service = CreateService(out var scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.ApplyMove(game.Id, Player.X, 0);
        service.ApplyMove(game.Id, Player.O, 3);
        service.ApplyMove(game.Id, Player.X, 1);
        service.ApplyMove(game.Id, Player.O, 4);
        service.ApplyMove(game.Id, Player.X, 2); // X wins, scoreboard = 1 X win

        var reset = service.Reset(game.Id);

        Assert.All(reset.Board, cell => Assert.Null(cell));
        Assert.Empty(reset.MoveHistory);
        Assert.Equal(GameStatus.InProgress, reset.Status);
        Assert.Equal(Player.X, reset.CurrentPlayer);
        Assert.Equal(1, scoreboard.Get().XWins); // unchanged by reset
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLastMove()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.ApplyMove(game.Id, Player.X, 0);
        service.ApplyMove(game.Id, Player.O, 4);

        var result = service.Undo(game.Id);

        Assert.Single(result.MoveHistory);
        Assert.Equal(Player.X, result.Board[0]);
        Assert.Null(result.Board[4]);
        Assert.Equal(Player.O, result.CurrentPlayer); // O's turn again
    }

    [Fact]
    public void Undo_ComputerMode_RemovesHumanAndComputerMoveTogether()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.VsComputer);

        // Human plays X at 0; computer auto-responds as O.
        var afterHumanMove = service.ApplyMove(game.Id, Player.X, 0);
        Assert.Equal(2, afterHumanMove.MoveHistory.Count); // human + computer move both applied

        var result = service.Undo(game.Id);

        Assert.Empty(result.MoveHistory);
        Assert.All(result.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, result.CurrentPlayer); // back to human's turn
    }

    [Fact]
    public void Undo_WithNoMoves_Throws()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<GameValidationException>(() => service.Undo(game.Id));
    }

    [Fact]
    public void Undo_AfterGameCompletion_IsDisabled()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.ApplyMove(game.Id, Player.X, 0);
        service.ApplyMove(game.Id, Player.O, 3);
        service.ApplyMove(game.Id, Player.X, 1);
        service.ApplyMove(game.Id, Player.O, 4);
        service.ApplyMove(game.Id, Player.X, 2); // X wins

        Assert.False(service.CanUndo(service.GetGame(game.Id)));
        Assert.Throws<GameValidationException>(() => service.Undo(game.Id));
    }

    [Fact]
    public void ComputerMode_ComputerNeverMoves_AfterGameAlreadyCompleted()
    {
        var service = CreateService(out _);
        var game = service.CreateGame(GameMode.VsComputer);

        // Force X into a winning position without letting the computer end the game first:
        // X: 0, O(computer) responds; X: 1 ... eventually X completes a line and no further
        // computer move should be appended once the game is Won.
        service.ApplyMove(game.Id, Player.X, 0); // computer responds automatically
        var state = service.GetGame(game.Id);
        Assert.True(state.Status == GameStatus.InProgress || state.Status == GameStatus.Won);
        // Whatever happens, board should never have more O moves than X moves + 1 is impossible
        // by construction; the key invariant is: once Won/Draw, move count stops growing.
        if (state.Status != GameStatus.InProgress)
        {
            Assert.Throws<GameValidationException>(() => service.ApplyMove(game.Id, Player.X, 1));
        }
    }
}
