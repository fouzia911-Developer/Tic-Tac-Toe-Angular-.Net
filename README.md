# Tic Tac Toe — Angular + .NET Web API

A browser-based Tic Tac Toe game built for the ABB Full Stack Developer take-home
assignment. Angular (standalone components) frontend, ASP.NET Core Web API backend,
in-memory state, REST communication.

## Project Overview

- Two game modes: **Two Player** and **Play Against Computer**.
- The **.NET backend is the single source of truth**: board state, turn, move
  history, win/draw detection, undo, and the session-level scoreboard all live
  and are computed on the server. The Angular app only renders whatever state
  the backend returns and calls REST endpoints for every action.
- Computer opponent uses a simple, deterministic priority strategy (win > block
  > center > corner > any cell).

## Tech Stack

| Layer     | Technology                                   |
| --------- | --------------------------------------------- |
| Frontend  | Angular 17 (standalone components), TypeScript |
| Backend   | ASP.NET Core 8 Web API (C#)                   |
| API style | REST (JSON)                                   |
| Storage   | In-memory (per-process, no database required) |
| Testing   | xUnit (backend), Jasmine/Karma (frontend)      |

## Features Implemented

- 3×3 board with click-to-play cells, locked once filled.
- Turn indicator, alternating turns, invalid moves rejected without changing turn.
- Win detection (rows, columns, diagonals) with winning-cell highlight.
- Draw detection when the board fills with no winner.
- Move history table (Move #, Player, Row/Column).
- Undo Last Move:
  - **Two Player Mode** — removes only the most recent move.
  - **Computer Mode** — removes the computer's move and the preceding human move together.
  - Disabled when there are no moves, or once the game has completed (see Design Decisions).
- Session-level Scoreboard (X wins / O wins / Draws), served by the backend, updated exactly once per completed game.
- Reset Game (keeps scoreboard) and Reset Scoreboard (independent action).
- Computer opponent (O) that auto-moves after the human (X) using the specified priority order.

## Repository Structure

```
tic-tac-toe/
├── backend/
│   ├── TicTacToe.Api/            # ASP.NET Core Web API
│   └── TicTacToe.Api.Tests/      # xUnit tests
├── frontend/
│   └── tic-tac-toe-client/       # Angular app
└── README.md
```

## How to Run the Backend Locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
cd backend/TicTacToe.Api
dotnet restore
dotnet run
```

The API listens on **http://localhost:5000** (see `Properties/launchSettings.json`).
Swagger UI is available at **http://localhost:5000/swagger** for exploring/testing
endpoints directly.

## How to Run the Frontend Locally

Requires [Node.js 18+](https://nodejs.org/) and npm.

```bash
cd frontend/tic-tac-toe-client
npm install
npm start
```

The app runs on **http://localhost:4200** and expects the backend at
`http://localhost:5000/api` (configured in `src/environments/environment.ts`).
Start the backend first (or in parallel) so the initial "create game" call succeeds.

## API Endpoint Summary

Base URL: `http://localhost:5000/api`

| Method | Endpoint                    | Purpose                                   |
| ------ | ---------------------------- | ------------------------------------------ |
| POST   | `/games`                     | Create a new game session. Body: `{ "mode": "TwoPlayer" \| "VsComputer" }` |
| GET    | `/games/{id}`                | Get current game state                     |
| POST   | `/games/{id}/moves`          | Submit a move. Body: `{ "player": "X" \| "O", "cellIndex": 0-8 }` |
| POST   | `/games/{id}/undo`           | Undo last move (or move-pair in Computer Mode) |
| POST   | `/games/{id}/reset`          | Reset the current game (scoreboard untouched) |
| GET    | `/scoreboard`                | Get the scoreboard                         |
| POST   | `/scoreboard/reset`          | Reset the scoreboard to zero               |

**Game State response shape:**

```json
{
  "gameId": "guid",
  "board": ["X", null, "O", null, null, null, null, null, null],
  "currentPlayer": "X",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": null,
  "moveHistory": [
    { "moveNumber": 1, "player": "X", "cellIndex": 0, "row": 1, "column": 1 }
  ],
  "canUndo": true,
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 }
}
```

Validation errors (invalid move, wrong turn, occupied cell, out-of-range
cell, move after completion, unknown game id) return `400`/`404` with:

```json
{ "message": "human-readable explanation" }
```

## How to Run Tests

**Backend (xUnit):**

```bash
cd backend
dotnet test
```

Covers: valid move, invalid move (occupied/out-of-range/wrong player), turn
switching, row/column/diagonal win, draw, reset, undo in two-player mode,
undo in computer mode, undo disabled after completion, scoreboard update
(once per game), and computer move selection at every priority level.

**Frontend (Jasmine/Karma):**

```bash
cd frontend/tic-tac-toe-client
npm test
```

Covers `GameService` API calls (all six endpoints) and `AppComponent`
rendering/status-message behavior.

## Design Decisions

- **Clarification 2 (Scoreboard & Undo) — Option A chosen:** Undo is disabled
  once a game is `Won` or `Draw`. This keeps the scoreboard update simple and
  always final for a completed game — no need to reverse a scoreboard entry,
  which avoids an entire class of edge cases (e.g., reversing a draw that
  becomes a win after undo-then-replay).
- **Computer move is applied server-side, synchronously, within the same
  `POST /moves` call** that submits the human's move. This keeps the frontend
  simple (it never needs to poll or manage a "computer is thinking" state) and
  keeps the backend as the sole authority over turn sequencing.
- **Cell addressing** uses a single `cellIndex` (0-8) end-to-end; row/column
  (1-based) are derived server-side for the move-history display, per the
  assignment's example table.
- **In-memory storage** via a singleton `GameService`/`ScoreboardService`
  (thread-safe with a simple lock) — sufficient for a local review exercise;
  state resets when the backend process restarts.
- **Standalone Angular components** (no NgModules) — smaller, more current
  Angular idiom; keeps the frontend footprint small for a focused review.

## Clarifications and Assumptions

- "Player" in the move request refers to the symbol (`X`/`O`), not a persisted
  user identity — there's no login/auth in scope for this exercise.
- In Computer Mode, the human always plays X and the computer always plays O,
  per the assignment.
- Switching mode (radio buttons in the UI) starts a brand-new game session;
  it does not attempt to convert an in-progress game between modes.
- CORS is restricted to `http://localhost:4200` for local development.

## Known Limitations

- No persistence layer — restarting the backend clears all games and the scoreboard.
- No multi-user/session isolation beyond the game's own GUID — anyone with a
  game ID could call its endpoints (acceptable for a local review exercise).
- Computer AI is intentionally simple (rule-based priority list), not a
  full minimax search, per the "basic computer opponent" requirement.

## Future Improvements

- Persist games/scoreboard to SQLite for durability across restarts.
- Add optimistic UI move rendering while awaiting the backend response.
- Add an unbeatable (minimax) computer difficulty option.
- Add end-to-end tests (Playwright/Cypress) covering the full user flow.

## AI-Assisted Development Summary

This solution was built with AI assistance (Claude). Summary of the workflow:

- **Specification:** Angular + .NET Web API Tic Tac Toe with: 3×3 board, 
- two-player and vs-computer modes, win/draw detection with highlighted winning cells,
- move history table, undo (single move in 2-player, move-pair in computer mode),
- a backend-owned scoreboard, reset game/scoreboard endpoints, 
- backend as source of truth, and unit tests for the core game logic.
- **Prompts used:** A single detailed prompt describing the assignment
  plus the instruction to
  act as a full-stack developer and produce a runnable solution with backend
  logic, frontend UI, tests, and documentation.
- **What the AI generated:** The full backend (models, services, controllers,
  tests) and frontend (components, services, models, tests) scaffolding and
  implementation, plus this README.- 
- **Assumptions and trade-offs:** documented above under "Design Decisions"
  and "Clarifications and Assumptions" — notably the Option A undo/scoreboard
  choice and the decision to run the computer's reply inside the same HTTP
  call as the human's move.
