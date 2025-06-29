using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Evolution.Core;

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

        public event System.Action<int, ulong> OnPlayerJoined;

        private void Awake()
        {
            if (NetworkManager.Singleton == null)
                Debug.LogWarning("LobbyManager requires a NetworkManager");
        }

        /// <summary>
        /// Create a new lobby and start hosting if needed.
        /// </summary>
        public Lobby CreateLobby(string name, int ownerId, GameType type, string difficulty, string password = null)
        {
            int id = nextLobbyId++;
            var lobby = new Lobby { LobbyId = id, Name = name, Password = password };
            lobbies[id] = lobby;

            lobby.Session = new SessionData
            {
                SessionId = id,
                OwnerId = ownerId,
                Type = type,
                Difficulty = difficulty,
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
        public bool JoinLobby(int lobbyId, ulong clientId, string password = null)
        {
            if (!lobbies.TryGetValue(lobbyId, out var lobby))
                return false;

            if (!string.IsNullOrEmpty(lobby.Password) && lobby.Password != password)
                return false;

            if (!lobby.Players.Contains(clientId))
            {
                lobby.Players.Add(clientId);
                lobby.Session?.Players.Add((int)clientId);
                OnPlayerJoined?.Invoke(lobbyId, clientId);
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
        public string Password;
        public SessionData Session;
    }
}
