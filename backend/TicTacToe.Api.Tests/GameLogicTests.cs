using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameLogicTests
{
    private static Player?[] EmptyBoard() => new Player?[9];

    [Fact]
    public void FindWinningLine_DetectsRowWin()
    {
        var board = EmptyBoard();
        board[0] = board[1] = board[2] = Player.X;

        var line = GameLogic.FindWinningLine(board, Player.X);

        Assert.NotNull(line);
        Assert.Equal(new[] { 0, 1, 2 }, line);
    }

    [Fact]
    public void FindWinningLine_DetectsColumnWin()
    {
        var board = EmptyBoard();
        board[1] = board[4] = board[7] = Player.O;

        var line = GameLogic.FindWinningLine(board, Player.O);

        Assert.NotNull(line);
        Assert.Equal(new[] { 1, 4, 7 }, line);
    }

    [Fact]
    public void FindWinningLine_DetectsDiagonalWin()
    {
        var board = EmptyBoard();
        board[0] = board[4] = board[8] = Player.X;

        var line = GameLogic.FindWinningLine(board, Player.X);

        Assert.NotNull(line);
        Assert.Equal(new[] { 0, 4, 8 }, line);
    }

    [Fact]
    public void FindWinningLine_ReturnsNull_WhenNoWin()
    {
        var board = EmptyBoard();
        board[0] = Player.X;
        board[1] = Player.O;

        Assert.Null(GameLogic.FindWinningLine(board, Player.X));
        Assert.Null(GameLogic.FindWinningLine(board, Player.O));
    }

    [Fact]
    public void IsBoardFull_TrueOnlyWhenAllCellsFilled()
    {
        var board = EmptyBoard();
        Assert.False(GameLogic.IsBoardFull(board));

        for (var i = 0; i < 9; i++)
        {
            board[i] = i % 2 == 0 ? Player.X : Player.O;
        }

        Assert.True(GameLogic.IsBoardFull(board));
    }

    [Fact]
    public void ComputerMove_TakesWinningMove_WhenAvailable()
    {
        // O has two in a row (3,4) and can win at 5.
        var board = EmptyBoard();
        board[3] = Player.O;
        board[4] = Player.O;
        board[0] = Player.X;
        board[1] = Player.X;

        var move = GameLogic.GetComputerMove(board);

        Assert.Equal(5, move);
    }

    [Fact]
    public void ComputerMove_BlocksOpponent_WhenNoWinAvailable()
    {
        // X has two in a column (0,3) threatening to win at 6.
        var board = EmptyBoard();
        board[0] = Player.X;
        board[3] = Player.X;
        board[4] = Player.O;

        var move = GameLogic.GetComputerMove(board);

        Assert.Equal(6, move);
    }

    [Fact]
    public void ComputerMove_TakesCenter_WhenNoWinOrBlockNeeded()
    {
        var board = EmptyBoard();
        board[0] = Player.X;

        var move = GameLogic.GetComputerMove(board);

        Assert.Equal(4, move);
    }

    [Fact]
    public void ComputerMove_TakesCorner_WhenCenterTaken()
    {
        var board = EmptyBoard();
        board[4] = Player.X; // center already taken

        var move = GameLogic.GetComputerMove(board);

        Assert.Contains(move, new[] { 0, 2, 6, 8 });
    }

    [Fact]
    public void ComputerMove_TakesAnyAvailableCell_AsLastResort()
    {
        // Only cell 5 is free, center and corners already taken.
        var board = new Player?[]
        {
            Player.X, Player.O, Player.X,
            Player.O, Player.O, null,
            Player.X, Player.O, Player.X
        };

        var move = GameLogic.GetComputerMove(board);

        Assert.Equal(5, move);
    }
}
