import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BoardComponent } from './components/board/board.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { GameMode, GameState } from './models/game.model';
import { GameService } from './services/game.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, BoardComponent, MoveHistoryComponent, ScoreboardComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  game: GameState | null = null;
  selectedMode: GameMode = 'TwoPlayer';
  errorMessage: string | null = null;
  loading = false;

  constructor(private readonly gameService: GameService) {}

  ngOnInit(): void {
    this.startNewGame(this.selectedMode);
  }

  get statusMessage(): string {
    if (!this.game) {
      return '';
    }
    if (this.game.status === 'Won') {
      return `Player ${this.game.winner} wins!`;
    }
    if (this.game.status === 'Draw') {
      return "It's a draw!";
    }
    const whoseTurn =
      this.game.mode === 'VsComputer' && this.game.currentPlayer === 'O'
        ? 'Computer (O)'
        : `Player ${this.game.currentPlayer}`;
    return `${whoseTurn}'s turn`;
  }

  get isGameOver(): boolean {
    return this.game?.status !== 'InProgress';
  }

  onModeChange(mode: GameMode): void {
    this.selectedMode = mode;
    this.startNewGame(mode);
  }

  startNewGame(mode: GameMode): void {
    this.loading = true;
    this.errorMessage = null;
    this.gameService.createGame(mode).subscribe({
      next: (state) => {
        this.game = state;
        this.loading = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  onCellClick(cellIndex: number): void {
    if (!this.game || this.isGameOver) {
      return;
    }
    this.errorMessage = null;
    this.gameService.submitMove(this.game.gameId, this.game.currentPlayer, cellIndex).subscribe({
      next: (state) => (this.game = state),
      error: (err) => this.handleError(err)
    });
  }

  onUndo(): void {
    if (!this.game) {
      return;
    }
    this.errorMessage = null;
    this.gameService.undo(this.game.gameId).subscribe({
      next: (state) => (this.game = state),
      error: (err) => this.handleError(err)
    });
  }

  onResetGame(): void {
    if (!this.game) {
      return;
    }
    this.errorMessage = null;
    this.gameService.resetGame(this.game.gameId).subscribe({
      next: (state) => (this.game = state),
      error: (err) => this.handleError(err)
    });
  }

  onResetScoreboard(): void {
    if (!this.game) {
      return;
    }
    this.gameService.resetScoreboard().subscribe({
      next: (scoreboard) => {
        if (this.game) {
          this.game = { ...this.game, scoreboard };
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  private handleError(err: HttpErrorResponse): void {
    this.loading = false;
    this.errorMessage = err.error?.message ?? 'Something went wrong talking to the server.';
  }
}
