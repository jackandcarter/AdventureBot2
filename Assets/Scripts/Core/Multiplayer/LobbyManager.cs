using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Evolution.Core.Multiplayer
{
    /// <summary>
    /// Manages multiplayer lobbies using Unity Netcode. Each lobby maps
    /// to a SessionData instance from the SessionManager.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        private readonly Dictionary<int, Lobby> lobbies = new();
        private int nextLobbyId = 1;

        [SerializeField] private SessionManager sessionManager;

        private void Awake()
        {
            if (NetworkManager.Singleton == null)
                Debug.LogWarning("LobbyManager requires a NetworkManager");
        }

        /// <summary>
        /// Create a new lobby and start hosting if needed.
        /// </summary>
        public Lobby CreateLobby(string name, int ownerId)
        {
            int id = nextLobbyId++;
            var lobby = new Lobby { LobbyId = id, Name = name };
            lobbies[id] = lobby;

            lobby.Session = new SessionData
            {
                SessionId = id,
                OwnerId = ownerId,
                Difficulty = "Normal",
                CurrentFloor = 1,
                CurrentPosition = Vector2Int.zero
            };
            sessionManager?.RegisterSession(lobby.Session);
            lobby.Session.Players.Add(ownerId);

            if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.StartHost();

            return lobby;
        }

        /// <summary>
        /// Get all currently active lobbies.
        /// </summary>
        public IReadOnlyCollection<Lobby> ListLobbies() => lobbies.Values;

        /// <summary>
        /// Join an existing lobby as a client.
        /// </summary>
        public bool JoinLobby(int lobbyId, ulong clientId)
        {
            if (!lobbies.TryGetValue(lobbyId, out var lobby))
                return false;

            if (!lobby.Players.Contains(clientId))
            {
                lobby.Players.Add(clientId);
                lobby.Session?.Players.Add((int)clientId);
            }

            if (!NetworkManager.Singleton.IsClient)
                NetworkManager.Singleton.StartClient();

            return true;
        }

        /// <summary>
        /// Leave a lobby. If the last player leaves, the lobby is removed.
        /// </summary>
        public void LeaveLobby(int lobbyId, ulong clientId)
        {
            if (!lobbies.TryGetValue(lobbyId, out var lobby))
                return;
            lobby.Players.Remove(clientId);
            lobby.Session?.Players.Remove((int)clientId);
            if (lobby.Players.Count == 0)
                lobbies.Remove(lobbyId);
        }
    }

    public class Lobby
    {
        public int LobbyId;
        public string Name;
        public List<ulong> Players = new();
        public SessionData Session;
    }
}
