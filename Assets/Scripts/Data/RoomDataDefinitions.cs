using System.Collections.Generic;
using UnityEngine;
using Evolution.Dungeon;
using Evolution.Inventory;
using Evolution.Combat;

namespace Evolution.Data
{
    [System.Serializable]
    public class RoomDefinition
    {
        public RoomType Type;
        public RoomPrefab Prefab;
        [TextArea]
        public string Description;
    }

    [CreateAssetMenu(fileName = "RoomDatabase", menuName = "Evolution/Room Database")]
    public class RoomDatabase : ScriptableObject
    {
        public List<RoomDefinition> Rooms = new();
    }

    [System.Serializable]
    public class EnemyStats
    {
        public string Name;
        public int MaxHp = 10;
        public int Attack = 1;
        public int Defense = 0;
        public float Speed = 1f;
        public List<Ability> Abilities = new();
    }

    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Evolution/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        public List<EnemyStats> Enemies = new();
    }

    [System.Serializable]
    public class ShopInventory
    {
        public string ShopName;
        public List<ItemData> Items = new();
    }

    [CreateAssetMenu(fileName = "ShopDatabase", menuName = "Evolution/Shop Database")]
    public class ShopDatabase : ScriptableObject
    {
        public List<ShopInventory> Shops = new();
    }

    [System.Serializable]
    public class PlayerClass
    {
        public string ClassName;
        public int BaseHp = 10;
        public int BaseAttack = 1;
        public int BaseDefense = 0;
        public float BaseSpeed = 1f;
        public List<Ability> Abilities = new();
    }

    [CreateAssetMenu(fileName = "ClassDatabase", menuName = "Evolution/Class Database")]
    public class ClassDatabase : ScriptableObject
    {
        public List<PlayerClass> Classes = new();
    }

    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Evolution/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> Items = new();
    }
}
