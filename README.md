# Evolution

This directory contains a Unity project targeting **Unity 6.1** with the **Universal Render Pipeline (URP)** enabled.

Scripts live under `Assets/Scripts/` and are organized into high level namespaces such as `Core`, `Dungeon`, `Combat`, `Inventory`, `UI`, `Data` and `Editor`.



This project is for development of AdventureGame for discord activites games platform. AdventureGame will be a MMORPG Dungeon Crawler, complete with procedural generation on modular prefabs and room templates that players will be able to traverse in order to reach the end goal/chamber. 

The dungeon will consist of Safe, Monster, Item/treasure Chest, Shop, NPC/Quest, Illusion, and Staircase up/down rooms. Some Rooms are also locked behind a door that requires a key from a chest to open. 

The Battle system is turn based and starts immediately when a player enters a room with a monster in it. Once the battle finishes the victory window appears and clicking continue will award the player their exp and items, etc. Players can level up and have a stat based JRPG Ratio formula system and speed system with elemental resist and weakness system as well as status effects with buffs, debuffs, DoT, HoT, and more.

### Multiplayer Lobbies

Lobbies are managed by the `LobbyManager` component found under
`Assets/Scripts/Core/Multiplayer`. It relies on Unity Netcode's
`NetworkManager` for connections and integrates with `SessionManager` so
each lobby maintains its own `SessionData`.

```
var lobby = lobbyManager.CreateLobby("My Lobby", ownerId, GameType.Multiplayer, "Easy", "secret");
var all = lobbyManager.ListLobbies();
lobbyManager.JoinLobby(lobby.LobbyId, clientId, "secret");
lobbyManager.LeaveLobby(lobby.LobbyId, clientId);
```

Add a `NetworkManager`, `SessionManager` and `LobbyManager` to a scene to
host or join sessions.

For a step-by-step tutorial on creating a playable test scene and required assets, see [`Assets/Docs/SetupGuide.md`](Assets/Docs/SetupGuide.md).

### LobbyUI

`LobbyUI` provides a basic lobby browser and host controls. It queries
`LobbyManager.ListLobbies()` and populates a scrollable list with join
buttons. A create lobby form allows specifying a name and optional password.
Hosts can start the game at any time (up to eight players) which will load the
configured gameplay scene for all members while keeping the lobby joinable.

Joining a lobby now displays a class selection panel. Players choose from the
classes defined in `ClassDatabase`, review their stats and confirm before
spawning. The chosen class is applied through `GameManager.BuildPlayer` when the
game starts.

