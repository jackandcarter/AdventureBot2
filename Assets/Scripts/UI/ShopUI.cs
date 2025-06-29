using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Evolution.Core;
using Evolution.Data;

namespace Evolution.UI
{
    /// <summary>
    /// Simple shop interface that displays purchasable items and notifies the
    /// GameManager when an item is bought.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private DataManager dataManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CanvasGroup shopCanvas;
        [SerializeField] private Button itemButtonPrefab;
        [SerializeField] private Transform buttonRoot;

        private readonly List<Button> spawned = new();

        /// <summary>
        /// Opens the given shop and populates item buttons.
        /// </summary>
        /// <param name="shop">Shop inventory to display.</param>
        public void OpenShop(ShopInventory shop)
        {
            Clear();
            if (shop == null || itemButtonPrefab == null || buttonRoot == null)
                return;

            if (shopCanvas != null)
            {
                shopCanvas.alpha = 1f;
                shopCanvas.interactable = true;
                shopCanvas.blocksRaycasts = true;
            }

            foreach (var item in shop.Items.Where(i => i != null))
            {
                var btn = Instantiate(itemButtonPrefab, buttonRoot);
                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = item.Name;
                var data = item; // local capture
                btn.onClick.AddListener(() => PurchaseItem(data));
                spawned.Add(btn);
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the shop interface and clears all buttons.
        /// </summary>
        public void CloseShop()
        {
            if (shopCanvas != null)
            {
                shopCanvas.alpha = 0f;
                shopCanvas.interactable = false;
                shopCanvas.blocksRaycasts = false;
            }
            Clear();
            gameObject.SetActive(false);
        }

        private void PurchaseItem(ItemData item)
        {
            if (gameManager != null && item != null)
                gameManager.AddItem(item);
        }

        private void Clear()
        {
            foreach (var b in spawned)
                if (b != null)
                    Destroy(b.gameObject);
            spawned.Clear();
        }

        public DataManager DataManager
        {
            get => dataManager;
            set => dataManager = value;
        }

        public GameManager GameManager
        {
            get => gameManager;
            set => gameManager = value;
        }
    }
}
