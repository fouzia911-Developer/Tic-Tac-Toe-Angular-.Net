using System.Text.Json.Serialization;

namespace TicTacToe.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Player
{
    X,
    O
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameMode
{
    TwoPlayer,
    VsComputer
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameStatus
{
    InProgress,
    Won,
    Draw
}
