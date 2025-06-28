using UnityEngine;

namespace Evolution.Dungeon
{
    /// <summary>
    /// Procedurally builds a simple rectangular room using tile models.
    /// </summary>
    public class RoomBuilder : MonoBehaviour
    {
        [SerializeField] private Vector2Int roomSize = new Vector2Int(10, 10);
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private string tileSetPath = "Models/Tiles/Base";

        private GameObject floorPrefab;
        private GameObject wallPrefab;
        private GameObject doorPrefab;

        private void Awake()
        {
            LoadPrefabs();
            Build();
        }

        private void LoadPrefabs()
        {
            if (floorPrefab == null)
                floorPrefab = Resources.Load<GameObject>($"{tileSetPath}/floor");
            if (wallPrefab == null)
                wallPrefab = Resources.Load<GameObject>($"{tileSetPath}/wall");
            if (doorPrefab == null)
                doorPrefab = Resources.Load<GameObject>($"{tileSetPath}/doorframe");
        }

        /// <summary>
        /// Instantiate the floor grid, enclosing walls and doorframes.
        /// </summary>
        public void Build()
        {
            if (floorPrefab == null || wallPrefab == null || doorPrefab == null)
                return;

            float halfWidth = roomSize.x * tileSize * 0.5f;
            float halfHeight = roomSize.y * tileSize * 0.5f;

            // floors
            for (int x = 0; x < roomSize.x; x++)
            {
                for (int y = 0; y < roomSize.y; y++)
                {
                    Vector3 pos = new Vector3((x + 0.5f) * tileSize - halfWidth, 0f, (y + 0.5f) * tileSize - halfHeight);
                    Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                }
            }

            // north/south walls
            for (int x = 0; x < roomSize.x; x++)
            {
                float offset = (x + 0.5f) * tileSize - halfWidth;
                Vector3 northPos = new Vector3(offset, 0f, halfHeight + tileSize * 0.5f);
                Vector3 southPos = new Vector3(offset, 0f, -halfHeight - tileSize * 0.5f);
                Instantiate(wallPrefab, northPos, Quaternion.identity, transform);
                Instantiate(wallPrefab, southPos, Quaternion.identity, transform);
            }

            // east/west walls
            for (int y = 0; y < roomSize.y; y++)
            {
                float offset = (y + 0.5f) * tileSize - halfHeight;
                Vector3 eastPos = new Vector3(halfWidth + tileSize * 0.5f, 0f, offset);
                Vector3 westPos = new Vector3(-halfWidth - tileSize * 0.5f, 0f, offset);
                Instantiate(wallPrefab, eastPos, Quaternion.Euler(0f, 90f, 0f), transform);
                Instantiate(wallPrefab, westPos, Quaternion.Euler(0f, 90f, 0f), transform);
            }

            // doorframes at +/- size/2 on each edge
            Instantiate(doorPrefab, new Vector3(0f, 0f, halfHeight + tileSize * 0.5f), Quaternion.identity, transform);
            Instantiate(doorPrefab, new Vector3(0f, 0f, -halfHeight - tileSize * 0.5f), Quaternion.identity, transform);
            Instantiate(doorPrefab, new Vector3(halfWidth + tileSize * 0.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), transform);
            Instantiate(doorPrefab, new Vector3(-halfWidth - tileSize * 0.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), transform);
        }
    }
}
