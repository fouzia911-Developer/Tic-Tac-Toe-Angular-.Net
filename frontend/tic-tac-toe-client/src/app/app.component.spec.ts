import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';
import { GameState } from './models/game.model';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let httpMock: HttpTestingController;

  const initialState: GameState = {
    gameId: 'game-1',
    board: Array(9).fill(null),
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: null,
    moveHistory: [],
    canUndo: false,
    scoreboard: { xWins: 0, oWins: 0, draws: 0 }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges(); // triggers ngOnInit -> createGame()

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/games`);
    req.flush(initialState);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  it('creates the app and requests an initial game on load', () => {
    expect(fixture.componentInstance).toBeTruthy();
    expect(fixture.componentInstance.game?.gameId).toBe('game-1');
  });

  it('renders the board and status message', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-board')).toBeTruthy();
    expect(compiled.textContent).toContain("Player X's turn");
  });

  it('shows the win message when the game is won', () => {
    fixture.componentInstance.game = { ...initialState, status: 'Won', winner: 'X' };
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Player X wins!');
  });
});
