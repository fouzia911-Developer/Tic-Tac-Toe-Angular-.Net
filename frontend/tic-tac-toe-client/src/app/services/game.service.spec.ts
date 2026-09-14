import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { GameState } from '../models/game.model';
import { GameService } from './game.service';

describe('GameService', () => {
  let service: GameService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiBaseUrl;

  const sampleState: GameState = {
    gameId: 'abc-123',
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

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [GameService]
    });
    service = TestBed.inject(GameService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('createGame() POSTs the selected mode', () => {
    service.createGame('VsComputer').subscribe((state) => {
      expect(state).toEqual(sampleState);
    });

    const req = httpMock.expectOne(`${baseUrl}/games`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ mode: 'VsComputer' });
    req.flush(sampleState);
  });

  it('submitMove() POSTs player and cellIndex to the moves endpoint', () => {
    service.submitMove('abc-123', 'X', 4).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/games/abc-123/moves`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ player: 'X', cellIndex: 4 });
    req.flush(sampleState);
  });

  it('undo() POSTs to the undo endpoint', () => {
    service.undo('abc-123').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/games/abc-123/undo`);
    expect(req.request.method).toBe('POST');
    req.flush(sampleState);
  });

  it('resetGame() POSTs to the reset endpoint', () => {
    service.resetGame('abc-123').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/games/abc-123/reset`);
    expect(req.request.method).toBe('POST');
    req.flush(sampleState);
  });

  it('getScoreboard() GETs the scoreboard', () => {
    service.getScoreboard().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/scoreboard`);
    expect(req.request.method).toBe('GET');
    req.flush(sampleState.scoreboard);
  });

  it('resetScoreboard() POSTs to the scoreboard reset endpoint', () => {
    service.resetScoreboard().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/scoreboard/reset`);
    expect(req.request.method).toBe('POST');
    req.flush(sampleState.scoreboard);
  });
});
