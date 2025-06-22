using UnityEngine;

namespace Evolution.Data
{
    /// <summary>
    /// Serializable representation of an inventory item. In a full game
    /// this would likely include additional stats such as rarity or
    /// effects, but for now it just stores basic fields.
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public int Id;
        public string Name;
        [TextArea]
        public string Description;
    }
}
