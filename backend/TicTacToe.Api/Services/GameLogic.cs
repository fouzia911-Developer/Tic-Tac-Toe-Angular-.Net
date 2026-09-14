using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Stateless helpers for win/draw detection and computer move selection.
/// Kept separate from GameService so the rules can be unit tested in isolation.
/// </summary>
public static class GameLogic
{
    // All 8 possible winning lines (rows, columns, diagonals).
    public static readonly int[][] WinningLines =
    {
        new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // rows
        new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // columns
        new[] {0, 4, 8}, new[] {2, 4, 6}                   // diagonals
    };

    private static readonly int[] Corners = { 0, 2, 6, 8 };
    private const int Center = 4;

    /// <summary>Returns the winning line if the given board has a winner, else null.</summary>
    public static int[]? FindWinningLine(Player?[] board, Player player)
    {
        foreach (var line in WinningLines)
        {
            if (board[line[0]] == player && board[line[1]] == player && board[line[2]] == player)
            {
                return line;
            }
        }
        return null;
    }

    public static bool IsBoardFull(Player?[] board) => board.All(c => c.HasValue);

    /// <summary>
    /// Computer move priority:
    /// 1. Winning move for O
    /// 2. Blocking move against X
    /// 3. Center
    /// 4. Corner
    /// 5. Any available cell
    /// </summary>
    public static int GetComputerMove(Player?[] board)
    {
        var empty = Enumerable.Range(0, 9).Where(i => board[i] is null).ToList();
        if (empty.Count == 0)
        {
            throw new InvalidOperationException("No available cells for computer move.");
        }

        // 1. Winning move
        foreach (var i in empty)
        {
            var trial = (Player?[])board.Clone();
            trial[i] = Player.O;
            if (FindWinningLine(trial, Player.O) is not null)
            {
                return i;
            }
        }

        // 2. Block X
        foreach (var i in empty)
        {
            var trial = (Player?[])board.Clone();
            trial[i] = Player.X;
            if (FindWinningLine(trial, Player.X) is not null)
            {
                return i;
            }
        }

        // 3. Center
        if (empty.Contains(Center))
        {
            return Center;
        }

        // 4. Corner
        var availableCorner = Corners.FirstOrDefault(c => empty.Contains(c), -1);
        if (availableCorner != -1)
        {
            return availableCorner;
        }

        // 5. Any remaining cell
        return empty.First();
    }
}
