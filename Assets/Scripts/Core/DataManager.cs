using UnityEngine;
using Evolution.Data;

namespace Evolution.Core
{
    /// <summary>
    /// Loads core game databases from the Resources folder and exposes
    /// them for other systems to query.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        [SerializeField] private RoomDatabase roomDatabase;
        [SerializeField] private EnemyDatabase enemyDatabase;
        [SerializeField] private ShopDatabase shopDatabase;
        [SerializeField] private ClassDatabase classDatabase;
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private StatsDatabase statsDatabase;

        private const string DataPath = "Data/";

        private void Awake()
        {
            if (roomDatabase == null)
                roomDatabase = Resources.Load<RoomDatabase>(DataPath + "RoomDatabase");
            if (enemyDatabase == null)
                enemyDatabase = Resources.Load<EnemyDatabase>(DataPath + "EnemyDatabase");
            if (shopDatabase == null)
                shopDatabase = Resources.Load<ShopDatabase>(DataPath + "ShopDatabase");
            if (classDatabase == null)
                classDatabase = Resources.Load<ClassDatabase>(DataPath + "ClassDatabase");
            if (itemDatabase == null)
                itemDatabase = Resources.Load<ItemDatabase>(DataPath + "ItemDatabase");
            if (statsDatabase == null)
                statsDatabase = Resources.Load<StatsDatabase>(DataPath + "StatsDatabase");
        }

        public RoomDatabase GetRoomDatabase() => roomDatabase;
        public EnemyDatabase GetEnemyDatabase() => enemyDatabase;
        public ShopDatabase GetShopDatabase() => shopDatabase;
        public ClassDatabase GetClassDatabase() => classDatabase;
        public ItemDatabase GetItemDatabase() => itemDatabase;
        public StatsDatabase GetStatsDatabase() => statsDatabase;
    }
}
