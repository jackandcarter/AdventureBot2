using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Evolution.Dungeon;
using Evolution.Combat;
using Evolution.Data;
using Evolution.UI;
using System.Linq;
using Unity.Netcode;
using Evolution.Core.Multiplayer;

namespace Evolution.Core
{
    /// <summary>
    /// Top level orchestrator for dungeon exploration. Inspired by the
    /// Python GameMaster class, this version drives gameplay inside Unity
    /// with all data stored locally for now.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private DungeonGenerator generator;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private SessionManager sessionManager;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private ShopUI shopUI;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private MapUI mapUI;
        [SerializeField] private LobbyManager lobbyManager;
        [SerializeField] private Multiplayer.PlayerController playerPrefab;
        [SerializeField] private string difficulty = "Easy";
        [SerializeField] private GameType gameType = GameType.Solo;

        /// <summary>Selected difficulty for the next session.</summary>
        public string Difficulty { get => difficulty; set => difficulty = value; }

        /// <summary>Selected game type for the next session.</summary>
        public GameType GameType { get => gameType; set => gameType = value; }

        /// <summary>Class chosen during setup for the local player.</summary>
        public PlayerClass SelectedClass { get => selectedClass; set => selectedClass = value; }

        [Serializable]
        private class RoomPrefabEntry
        {
            public RoomType Type;
            public RoomPrefab Prefab;
        }

        [SerializeField] private List<RoomPrefabEntry> roomPrefabs = new();
        [SerializeField] private Transform dungeonRoot;

        private readonly List<RoomPrefab> spawnedRooms = new();
        private readonly Dictionary<Vector2Int, RoomPrefab> roomLookup = new();
        private readonly Dictionary<RoomType, RoomPrefab> prefabLookup = new();

        public event Action<RoomData> OnRoomEntered;
        public event Action<DungeonData> OnDungeonLoaded;

        private SessionData currentSession;
        private Player player;
        private PlayerClass selectedClass;
        private readonly Evolution.Inventory.Inventory inventory = new();

        private void Awake()
        {
            BuildPrefabLookup();
            if (battleManager != null)
                battleManager.OnBattleEnded += HandleBattleEnded;
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            if (lobbyManager != null)
                lobbyManager.OnPlayerJoined += HandleLobbyJoined;
        }

        private void OnDestroy()
        {
            if (battleManager != null)
                battleManager.OnBattleEnded -= HandleBattleEnded;
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            if (lobbyManager != null)
                lobbyManager.OnPlayerJoined -= HandleLobbyJoined;
        }

        public void BuildPlayer(int ownerId, PlayerClass cls)
        {
            player = new Player { Id = ownerId };
            if (cls != null)
            {
                player.ClassName = cls.ClassName;
                foreach (var cs in cls.Stats)
                    if (cs != null && cs.Stat != null)
                        player.SetStat(cs.Stat.Name, cs.Value);
                player.Abilities = new List<Ability>(cls.Abilities);
            }

            if (dataManager != null)
            {
                var statsDb = dataManager.GetStatsDatabase();
                if (statsDb != null)
                {
                    foreach (var def in statsDb.Stats)
                        if (def != null && !player.Stats.ContainsKey(def.Name))
                            player.SetStat(def.Name, def.DefaultValue);
                }
            }

            player.CurrentHp = player.GetStat("MaxHp");
        }

        private void BuildPlayer(int ownerId)
        {
            var classDb = dataManager != null ? dataManager.GetClassDatabase() : null;
            var cls = classDb != null && classDb.Classes.Count > 0 ? classDb.Classes[0] : null;
            BuildPlayer(ownerId, cls);
        }

        private void HandleBattleEnded(bool victory)
        {
            if (player == null) return;
            if (battleManager != null && battleManager.State.Players.TryGetValue(player.Id, out var c))
                player.CurrentHp = c.Hp;
            if (victory && dataManager != null)
                player.AddExperience(10f, dataManager.GetStatsDatabase());
        }

        public void StartNewGame(int ownerId)
        {
            if (sessionManager == null || generator == null)
            {
                Debug.LogError("GameManager missing dependencies");
                return;
            }
            BuildPrefabLookup();
            var cls = selectedClass;
            if (cls == null && dataManager != null)
            {
                var db = dataManager.GetClassDatabase();
                if (db != null && db.Classes.Count > 0)
                    cls = db.Classes[0];
            }
            BuildPlayer(ownerId, cls);
            currentSession = new SessionData
            {
                SessionId = ownerId, // temporary id if no DB yet
                OwnerId = ownerId,
                Type = gameType,
                Difficulty = difficulty,
                CurrentFloor = 1,
                CurrentPosition = Vector2Int.zero,
                Dungeon = generator.GenerateDungeon()
            };
            sessionManager.RegisterSession(currentSession);
            SpawnFloor(currentSession.CurrentFloor - 1);
            OnDungeonLoaded?.Invoke(currentSession.Dungeon);
            EnterRoom(currentSession.CurrentPosition);

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(id) == null)
                        SpawnPlayerForClient(id);
                }
            }
        }

        public void LoadSession(int sessionId)
        {
            if (sessionManager == null) return;
            currentSession = sessionManager.LoadSession(sessionId);
            if (currentSession != null)
            {
                BuildPrefabLookup();
                SpawnFloor(currentSession.CurrentFloor - 1);
                OnDungeonLoaded?.Invoke(currentSession.Dungeon);
                EnterRoom(currentSession.CurrentPosition);
            }
            else
            {
                Debug.LogWarning($"Session {sessionId} not found");
            }
        }

        public void Move(Vector2Int delta)
        {
            if (currentSession == null) return;
            Vector2Int next = currentSession.CurrentPosition + delta;
            var floor = currentSession.Dungeon.Floors[currentSession.CurrentFloor - 1];
            foreach (var room in floor.Rooms)
            {
                if (room.Coord == next)
                {
                    currentSession.CurrentPosition = next;
                    EnterRoom(next);
                    sessionManager.SaveSession(currentSession);
                    if (mapUI != null)
                        mapUI.DrawFloor(currentSession.Dungeon.Floors[currentSession.CurrentFloor - 1], currentSession.CurrentPosition);
                    return;
                }
            }
            Debug.Log("Cannot move in that direction");
        }

        private void EnterRoom(Vector2Int coord)
        {
            var floor = currentSession.Dungeon.Floors[currentSession.CurrentFloor - 1];
            RoomData room = floor.Rooms.Find(r => r.Coord == coord);
            if (room == null) return;

            roomLookup.TryGetValue(coord, out var prefab);
            HandleRoom(room, prefab);
            OnRoomEntered?.Invoke(room);
        }

        private void HandleRoom(RoomData room, RoomPrefab roomPrefab)
        {
            switch (room.Type)
            {
                case RoomType.Monster:
                case RoomType.Boss:
                    if (battleManager != null)
                    {
                        Combatant enemy = null;
                        if (dataManager != null)
                        {
                            var db = dataManager.GetEnemyDatabase();
                            if (db != null && db.Enemies.Count > 0)
                            {
                                var stats = db.Enemies[Random.Range(0, db.Enemies.Count)];
                                enemy = new Combatant
                                {
                                    Id = 0,
                                    Hp = stats.MaxHp,
                                    MaxHp = stats.MaxHp,
                                    Attack = stats.Attack,
                                    Defense = stats.Defense,
                                    Speed = stats.Speed,
                                    Abilities = new List<Ability>(stats.Abilities)
                                };
                            }
                        }

                        enemy ??= new Combatant { Id = 0, Hp = 10, MaxHp = 10, Speed = 1f, Attack = 1, Defense = 0 };

                        var playerCombatant = player != null ? player.ToCombatant() : new Combatant { Id = currentSession.OwnerId, Hp = 10, MaxHp = 10, Speed = 1f, Attack = 1, Defense = 0 };
                        battleManager.StartBattle(enemy, new List<Combatant> { playerCombatant });
                    }
                    break;
                case RoomType.Locked:
                    HandleLockedRoom(roomPrefab);
                    break;
                case RoomType.Shop:
                    if (shopUI != null && dataManager != null)
                    {
                        var db = dataManager.GetShopDatabase();
                        if (db != null && db.Shops.Count > 0)
                            shopUI.OpenShop(db.Shops[0]);
                    }
                    break;
                case RoomType.Treasure:
                    Debug.Log("Found a treasure chest!");
                    if (dataManager != null)
                    {
                        var db = dataManager.GetItemDatabase();
                        if (db != null && db.Items.Count > 0)
                        {
                            var item = db.Items[Random.Range(0, db.Items.Count)];
                            AddItem(item);
                            Debug.Log($"Picked up {item.Name}");
                        }
                        else
                        {
                            Debug.Log("But it was empty...");
                        }
                    }
                    break;
                case RoomType.Illusion:
                    HandleIllusionRoom(room);
                    break;
                case RoomType.StaircaseDown:
                    currentSession.CurrentFloor++;
                    currentSession.CurrentPosition = Vector2Int.zero;
                    SpawnFloor(currentSession.CurrentFloor - 1);
                    OnDungeonLoaded?.Invoke(currentSession.Dungeon);
                    break;
                case RoomType.StaircaseUp:
                    currentSession.CurrentFloor = Math.Max(1, currentSession.CurrentFloor - 1);
                    currentSession.CurrentPosition = Vector2Int.zero;
                    SpawnFloor(currentSession.CurrentFloor - 1);
                    OnDungeonLoaded?.Invoke(currentSession.Dungeon);
                    break;
            }
        }

        private void HandleLockedRoom(RoomPrefab roomPrefab)
        {
            if (roomPrefab == null)
            {
                Debug.Log("Encountered a locked room but prefab was null.");
                return;
            }

            var door = roomPrefab.DoorMap.Values.FirstOrDefault(d => d != null);
            if (door == null)
            {
                Debug.Log("Encountered a locked door but none was found in the room prefab.");
                return;
            }

            if (door.IsLocked)
            {
                var keyItem = inventory.Items.FirstOrDefault(i => i.Name == "Key");
                if (keyItem != null)
                {
                    inventory.RemoveItem(keyItem);
                    door.Unlock();
                    door.Open();
                    Debug.Log("Used a key to unlock the door.");
                }
                else
                {
                    Debug.Log("The door is locked. A key is required.");
                }
            }
        }

        private void HandleIllusionRoom(RoomData room)
        {
            if (uiManager == null || room == null)
                return;

            // Randomly choose between a crystal puzzle or perception check
            bool crystalPuzzle = Random.value < 0.5f;
            if (crystalPuzzle)
            {
                var crystals = new List<object> { "Red", "Blue", "Green" };
                int correct = Random.Range(0, crystals.Count);
                uiManager.ShowIllusionCrystal(correct, crystals);
                bool solved = Random.Range(0, crystals.Count) == correct;
                if (solved)
                {
                    Debug.Log("You shatter the correct crystal and the illusion fades.");
                    uiManager.ShowRoom(room);
                }
                else
                {
                    Debug.Log("Nothing happens. The illusion remains.");
                }
            }
            else
            {
                var options = new List<int> { 1, 2, 3 };
                uiManager.ShowIllusionEnemyCount(options);
                bool success = Random.value > 0.5f;
                if (success)
                {
                    Debug.Log("You see through the illusion!");
                    uiManager.ShowRoom(room);
                }
                else
                {
                    Debug.Log("The phantasms confuse you.");
                }
            }
        }

        private void BuildPrefabLookup()
        {
            prefabLookup.Clear();
            foreach (var entry in roomPrefabs)
            {
                if (entry.Prefab != null)
                    prefabLookup[entry.Type] = entry.Prefab;
            }
        }

        private void ClearSpawned()
        {
            foreach (var r in spawnedRooms)
                if (r != null)
                    Destroy(r.gameObject);
            spawnedRooms.Clear();
            roomLookup.Clear();
        }

        private void SpawnFloor(int floorIndex)
        {
            if (currentSession == null || currentSession.Dungeon == null) return;
            if (floorIndex < 0 || floorIndex >= currentSession.Dungeon.Floors.Count) return;

            ClearSpawned();
            var floor = currentSession.Dungeon.Floors[floorIndex];
            foreach (var room in floor.Rooms)
            {
                if (!prefabLookup.TryGetValue(room.Type, out var prefab) || prefab == null)
                    continue;
                float spacing = prefab.RoomDimensions.x;
                Vector3 pos = new Vector3(room.Coord.x * spacing, 0f, room.Coord.y * spacing);
                var inst = Instantiate(prefab, pos, Quaternion.identity, dungeonRoot);
                SetupDoors(inst, room);
                spawnedRooms.Add(inst);
                roomLookup[room.Coord] = inst;
            }
            if (mapUI != null)
                mapUI.DrawFloor(currentSession.Dungeon.Floors[floorIndex], currentSession.CurrentPosition);
        }

        private void SetupDoors(RoomPrefab roomPrefab, RoomData data)
        {
            if (roomPrefab == null || data == null) return;

            roomPrefab.BuildDoorMap();

            foreach (var door in roomPrefab.DoorMap.Values)
                SetDoorActive(door, false);

            foreach (var conn in data.Connections)
            {
                Vector2Int dir = conn - data.Coord;
                if (roomPrefab.TryGetDoor(dir, out var door))
                    SetDoorActive(door, true);
            }
        }

        public void AddItem(ItemData item)
        {
            if (item != null)
                inventory.AddItem(item);
        }

        private void SetDoorActive(Door door, bool active)
        {
            if (door == null) return;
            door.gameObject.SetActive(active);
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;
            if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId) == null)
                SpawnPlayerForClient(clientId);
        }

        private void SpawnPlayerForClient(ulong clientId)
        {
            if (playerPrefab == null)
                return;
            var inst = Instantiate(playerPrefab);
            inst.NetworkObject.SpawnAsPlayerObject(clientId, true);
            if (currentSession != null && !currentSession.Players.Contains((int)clientId))
                currentSession.Players.Add((int)clientId);
        }

        private void HandleLobbyJoined(int lobbyId, ulong clientId)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                SpawnPlayerForClient(clientId);
        }
    }
}
