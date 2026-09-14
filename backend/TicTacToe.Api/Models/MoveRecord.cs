namespace TicTacToe.Api.Models;

/// <summary>
/// Represents a single move that was applied to the board.
/// Row/Column are zero-based (Row 0..2, Column 0..2) but are surfaced
/// to the frontend as 1-based for display purposes (see MoveHistoryDto).
/// </summary>
public class MoveRecord
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int CellIndex { get; set; } // 0-8
    public int Row => CellIndex / 3;
    public int Column => CellIndex % 3;
}
