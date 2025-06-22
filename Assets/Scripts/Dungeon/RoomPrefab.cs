using UnityEngine;

namespace Evolution.Dungeon
{
    /// <summary>
    /// Component placed on dungeon room prefabs so doors can be
    /// enabled based on connections during generation.
    /// </summary>
    public class RoomPrefab : MonoBehaviour
    {
        public Transform NorthDoor;
        public Transform EastDoor;
        public Transform SouthDoor;
        public Transform WestDoor;
    }
}
