using System.Collections.Generic;
using UnityEngine;

namespace Evolution.Dungeon
{
    /// <summary>
    /// Component placed on dungeon room prefabs so doors can be
    /// enabled based on connections during generation.
    /// </summary>
    public class RoomPrefab : MonoBehaviour
    {
        [Header("Door References")]
        public Door NorthDoor;
        public Door EastDoor;
        public Door SouthDoor;
        public Door WestDoor;

        private RoomBuilder builder;

        private readonly Dictionary<Vector2Int, Door> doorMap = new();

        /// <summary>
        /// Mapping from direction to door component for quick lookup.
        /// </summary>
        public IReadOnlyDictionary<Vector2Int, Door> DoorMap => doorMap;

        private void Awake()
        {
            BuildDoorMap();
            builder = GetComponent<RoomBuilder>();
        }

        /// <summary>
        /// Builds the directional door mapping. Can be called from
        /// external code if doors are modified at runtime.
        /// </summary>
        public void BuildDoorMap()
        {
            doorMap.Clear();
            if (NorthDoor != null) doorMap[Vector2Int.up] = NorthDoor;
            if (EastDoor != null) doorMap[Vector2Int.right] = EastDoor;
            if (SouthDoor != null) doorMap[Vector2Int.down] = SouthDoor;
            if (WestDoor != null) doorMap[Vector2Int.left] = WestDoor;
        }

        /// <summary>
        /// Try to get a door based on direction relative to this room.
        /// </summary>
        public bool TryGetDoor(Vector2Int direction, out Door door)
        {
            if (doorMap.Count == 0) BuildDoorMap();
            return doorMap.TryGetValue(direction, out door);
        }

        /// <summary>
        /// World dimensions of this room prefab, derived from its RoomBuilder.
        /// </summary>
        public Vector2 RoomDimensions
        {
            get
            {
                if (builder == null)
                    builder = GetComponent<RoomBuilder>();
                return builder != null ? builder.RoomDimensions : Vector2.one * 10f;
            }
        }
    }
}
