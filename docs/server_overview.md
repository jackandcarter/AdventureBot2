# Server Overview

This document outlines a minimal Python-based lobby management server for **AdventureBot2**. The goal is to allow multiple players to form parties and embark on dungeon runs while the server tracks sessions and party composition.

## Basic Requirements

- **Session Tracking**: Maintain a list of active dungeon sessions and their associated player parties.
- **Player Parties**: Support creating, updating, and dissolving parties.
- **Joining Mechanism**: Allow players to join and leave parties or sessions via network requests.
- **Communication**: Provide endpoints or channels for a Unity client to interact with the server.

## Minimal Server Design

The server can be built with Python's `fastapi` and `uvicorn` libraries for a lightweight RESTful API. WebSocket support can be added for real-time updates if desired.

### Data Model

- `Session`: Represents an active dungeon instance, identified by a unique ID.
  - `session_id`: Unique identifier (UUID or incrementing integer).
  - `party`: List of player IDs currently in the session.
  - `status`: State of the session (e.g., "waiting", "in_progress", "complete").

- `Party`: Represents a group of players preparing for or engaged in a session.
  - `party_id`: Unique identifier.
  - `players`: List of player IDs.
  - `session_id`: Optional reference to an active session once started.

### API Endpoints (REST)

- `POST /party` – Create a new party.
- `POST /party/{party_id}/join` – Add a player to a party.
- `POST /party/{party_id}/leave` – Remove a player from a party.
- `POST /session` – Create a new dungeon session for a specific party.
- `GET /session/{session_id}` – Retrieve session details, including current party members and status.
- `GET /sessions` – List all active sessions.

### WebSocket Channel

For real-time updates (player join/leave events, session progress, etc.), the server can expose a WebSocket endpoint, e.g. `ws://<server>/updates`. Clients subscribe to updates for their specific party or session ID.

## Communication from Unity Client

1. **REST API**: The Unity client issues HTTP requests to create parties, join sessions, and query state. Use `UnityWebRequest` to perform `GET` and `POST` calls.
2. **WebSocket** (optional): The client connects to the server's WebSocket endpoint to receive real-time notifications. Libraries such as `WebSocketSharp` (or Unity's built-in WebSocket support) can maintain the connection.

## Example Minimal Implementation Snippet

```python
from fastapi import FastAPI, WebSocket
from typing import Dict, List
from uuid import uuid4

app = FastAPI()

sessions: Dict[str, Dict] = {}
parties: Dict[str, Dict] = {}

@app.post("/party")
def create_party():
    party_id = str(uuid4())
    parties[party_id] = {"players": []}
    return {"party_id": party_id}

@app.post("/party/{party_id}/join")
def join_party(party_id: str, player_id: str):
    parties[party_id]["players"].append(player_id)
    return {"status": "joined"}

@app.post("/session")
def create_session(party_id: str):
    session_id = str(uuid4())
    sessions[session_id] = {"party": parties[party_id], "status": "waiting"}
    parties[party_id]["session_id"] = session_id
    return {"session_id": session_id}

@app.websocket("/updates")
async def websocket_endpoint(ws: WebSocket):
    await ws.accept()
    while True:
        data = await ws.receive_text()
        # handle subscription or broadcast messages here
```

## Summary

- The server manages parties and sessions in-memory (or in a database for production).
- REST endpoints provide the primary interface for creating parties and sessions.
- WebSockets enable real-time notifications for connected clients.
- The Unity client interacts with the server via HTTP and optionally WebSocket connections.


