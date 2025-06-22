using System;
using System.Collections.Generic;
using UnityEngine;
using Evolution.Dungeon;
using Evolution.Combat;
using Evolution.Inventory;
using System.Linq;

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
        [SerializeField] private string difficulty = "Easy";

        [Serializable]
        private class RoomPrefabEntry
        {
            public RoomType Type;
            public RoomPrefab Prefab;
        }

        [SerializeField] private List<RoomPrefabEntry> roomPrefabs = new();
        [SerializeField] private float roomSize = 10f;
        [SerializeField] private Transform dungeonRoot;

        private readonly List<RoomPrefab> spawnedRooms = new();
        private readonly Dictionary<RoomType, RoomPrefab> prefabLookup = new();

        public event Action<RoomData> OnRoomEntered;
        public event Action<DungeonData> OnDungeonLoaded;

        private SessionData currentSession;
        private readonly Inventory inventory = new();

        private void Awake()
        {
            BuildPrefabLookup();
        }

        public void StartNewGame(int ownerId)
        {
            if (sessionManager == null || generator == null)
            {
                Debug.LogError("GameManager missing dependencies");
                return;
            }
            BuildPrefabLookup();
            currentSession = new SessionData
            {
                SessionId = ownerId, // temporary id if no DB yet
                OwnerId = ownerId,
                Difficulty = difficulty,
                CurrentFloor = 1,
                CurrentPosition = Vector2Int.zero,
                Dungeon = generator.GenerateDungeon()
            };
            sessionManager.RegisterSession(currentSession);
            SpawnFloor(currentSession.CurrentFloor - 1);
            OnDungeonLoaded?.Invoke(currentSession.Dungeon);
            EnterRoom(currentSession.CurrentPosition);
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
            HandleRoom(room);
            OnRoomEntered?.Invoke(room);
        }

        private void HandleRoom(RoomData room)
        {
            switch (room.Type)
            {
                case RoomType.Monster:
                case RoomType.Boss:
                    if (battleManager != null)
                    {
                        var enemy = new Combatant { Id = 0, Hp = 10, MaxHp = 10, Speed = 1f };
                        var player = new Combatant { Id = currentSession.OwnerId, Hp = 10, MaxHp = 10, Speed = 1f };
                        battleManager.StartBattle(enemy, new List<Combatant> { player });
                    }
                    break;
                case RoomType.Locked:
                    HandleLockedRoom();
                    break;
                case RoomType.Treasure:
                    Debug.Log("Found a treasure chest!");
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

        private void HandleLockedRoom()
        {
            var door = FindObjectOfType<Evolution.Dungeon.Door>();
            if (door == null)
            {
                Debug.Log("Encountered a locked door but none was found in the scene.");
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
                Vector3 pos = new Vector3(room.Coord.x * roomSize, 0f, room.Coord.y * roomSize);
                var inst = Instantiate(prefab, pos, Quaternion.identity, dungeonRoot);
                SetupDoors(inst, room);
                spawnedRooms.Add(inst);
            }
        }

        private void SetupDoors(RoomPrefab roomPrefab, RoomData data)
        {
            if (roomPrefab == null || data == null) return;

            if (roomPrefab.NorthDoor != null) roomPrefab.NorthDoor.gameObject.SetActive(false);
            if (roomPrefab.EastDoor != null) roomPrefab.EastDoor.gameObject.SetActive(false);
            if (roomPrefab.SouthDoor != null) roomPrefab.SouthDoor.gameObject.SetActive(false);
            if (roomPrefab.WestDoor != null) roomPrefab.WestDoor.gameObject.SetActive(false);

            foreach (var conn in data.Connections)
            {
                Vector2Int dir = conn - data.Coord;
                if (dir == Vector2Int.up && roomPrefab.NorthDoor != null)
                    roomPrefab.NorthDoor.gameObject.SetActive(true);
                else if (dir == Vector2Int.right && roomPrefab.EastDoor != null)
                    roomPrefab.EastDoor.gameObject.SetActive(true);
                else if (dir == Vector2Int.down && roomPrefab.SouthDoor != null)
                    roomPrefab.SouthDoor.gameObject.SetActive(true);
                else if (dir == Vector2Int.left && roomPrefab.WestDoor != null)
                    roomPrefab.WestDoor.gameObject.SetActive(true);
            }
        }
    }
}
