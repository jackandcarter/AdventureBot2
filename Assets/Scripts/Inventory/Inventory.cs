using System.Collections.Generic;
using Evolution.Data;

namespace Evolution.Inventory
{
    /// <summary>
    /// Basic container for player items. Provides simple add/remove
    /// operations and exposes a read-only view of contained items.
    /// </summary>
    [System.Serializable]
    public class Inventory
    {
        private readonly List<ItemData> items = new();

        public IReadOnlyList<ItemData> Items => items;

        public void AddItem(ItemData item)
        {
            if (item != null)
                items.Add(item);
        }

        public bool RemoveItem(ItemData item)
        {
            return items.Remove(item);
        }
    }
}
