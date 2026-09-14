export type PlayerSymbol = 'X' | 'O';

export type GameMode = 'TwoPlayer' | 'VsComputer';

export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface MoveHistoryEntry {
  moveNumber: number;
  player: PlayerSymbol;
  cellIndex: number;
  row: number;    // 1-based, for display
  column: number; // 1-based, for display
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  gameId: string;
  board: (PlayerSymbol | null)[];
  currentPlayer: PlayerSymbol;
  mode: GameMode;
  status: GameStatus;
  winner: PlayerSymbol | null;
  winningCells: number[] | null;
  moveHistory: MoveHistoryEntry[];
  canUndo: boolean;
  scoreboard: Scoreboard;
}

export interface ApiError {
  message: string;
}
