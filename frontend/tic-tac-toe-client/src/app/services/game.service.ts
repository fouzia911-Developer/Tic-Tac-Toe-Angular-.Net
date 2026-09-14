import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GameMode, GameState, PlayerSymbol, Scoreboard } from '../models/game.model';

/**
 * Thin wrapper around the .NET backend's REST API. The backend is the single
 * source of truth for game state (see README "Clarification 1") - this
 * service never computes game rules itself, it only relays requests and
 * returns whatever GameState the backend responds with.
 */
@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  createGame(mode: GameMode): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games`, { mode });
  }

  getGame(gameId: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.baseUrl}/games/${gameId}`);
  }

  submitMove(gameId: string, player: PlayerSymbol, cellIndex: number): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/moves`, {
      player,
      cellIndex
    });
  }

  undo(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/undo`, {});
  }

  resetGame(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/reset`, {});
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.baseUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.baseUrl}/scoreboard/reset`, {});
  }
}
