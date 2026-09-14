import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PlayerSymbol } from '../../models/game.model';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.component.html',
  styleUrl: './board.component.scss'
})
export class BoardComponent {
  @Input() board: (PlayerSymbol | null)[] = Array(9).fill(null);
  @Input() winningCells: number[] | null = null;
  @Input() disabled = false;

  @Output() cellClicked = new EventEmitter<number>();

  isWinningCell(index: number): boolean {
    return !!this.winningCells?.includes(index);
  }

  onCellClick(index: number): void {
    if (this.disabled || this.board[index] !== null) {
      return;
    }
    this.cellClicked.emit(index);
  }
}
