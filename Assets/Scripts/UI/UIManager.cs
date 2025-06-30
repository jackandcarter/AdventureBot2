using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Evolution.Dungeon;

namespace Evolution.UI
{
    /// <summary>
    /// Controls high level user interface canvases such as room descriptions,
    /// battle logs and the main menu. Visual elements can be themed via the
    /// inspector.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup roomCanvas;
        [SerializeField] private CanvasGroup battleCanvas;
        [SerializeField] private CanvasGroup menuCanvas;

        [Header("Room Elements")]
        [SerializeField] private TMP_Text roomDescription;

        [Header("Battle Elements")]
        [SerializeField] private TMP_Text battleLog;

        [Header("Theme")]
        [SerializeField] private Color themeColor = Color.white;
        [SerializeField] private TMP_FontAsset themeFont;

        public Color ThemeColor { get => themeColor; set { themeColor = value; ApplyTheme(); } }
        public TMP_FontAsset ThemeFont { get => themeFont; set { themeFont = value; ApplyTheme(); } }

        private void Awake()
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            if (roomDescription != null)
            {
                roomDescription.color = themeColor;
                if (themeFont != null) roomDescription.font = themeFont;
            }
            if (battleLog != null)
            {
                battleLog.color = themeColor;
                if (themeFont != null) battleLog.font = themeFont;
            }
        }

        public void ShowRoom(RoomData room)
        {
            if (roomCanvas != null)
            {
                roomCanvas.alpha = 1f;
                roomCanvas.interactable = true;
                roomCanvas.blocksRaycasts = true;
            }
            if (battleCanvas != null)
            {
                battleCanvas.alpha = 0f;
                battleCanvas.interactable = false;
                battleCanvas.blocksRaycasts = false;
            }
            if (menuCanvas != null)
            {
                menuCanvas.alpha = 0f;
                menuCanvas.interactable = false;
                menuCanvas.blocksRaycasts = false;
            }
            if (roomDescription != null && room != null)
                roomDescription.text = $"{room.Type}: {room.Coord}";
        }

        public void ShowBattle(string log)
        {
            if (battleCanvas != null)
            {
                battleCanvas.alpha = 1f;
                battleCanvas.interactable = true;
                battleCanvas.blocksRaycasts = true;
            }
            if (roomCanvas != null)
            {
                roomCanvas.alpha = 0f;
                roomCanvas.interactable = false;
                roomCanvas.blocksRaycasts = false;
            }
            if (battleLog != null)
                battleLog.text = log;
        }

        public void ShowMenu()
        {
            if (menuCanvas != null)
            {
                menuCanvas.alpha = 1f;
                menuCanvas.interactable = true;
                menuCanvas.blocksRaycasts = true;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Placeholder methods for illusion rooms based on embed_manager.py
        // lines 568‑576. These will be implemented in a future iteration.
        // ──────────────────────────────────────────────────────────────
        public void ShowIllusionRoom()
        {
            if (roomCanvas != null)
            {
                roomCanvas.alpha = 1f;
                roomCanvas.interactable = true;
                roomCanvas.blocksRaycasts = true;
            }
            if (battleCanvas != null)
            {
                battleCanvas.alpha = 0f;
                battleCanvas.interactable = false;
                battleCanvas.blocksRaycasts = false;
            }
            if (menuCanvas != null)
            {
                menuCanvas.alpha = 0f;
                menuCanvas.interactable = false;
                menuCanvas.blocksRaycasts = false;
            }
            if (roomDescription != null)
                roomDescription.text = "You sense a powerful illusion in this room.";
        }

        public void ShowIllusionCrystal(int index, IList<object> crystals)
        {
            ShowIllusionRoom();
            if (roomDescription == null)
                return;

            string label = "Unknown";
            if (crystals != null && index >= 0 && index < crystals.Count && crystals[index] != null)
                label = crystals[index].ToString();

            roomDescription.text = $"Crystal {index + 1}: {label}";
        }

        public void ShowIllusionEnemyCount(IList<int> options)
        {
            ShowIllusionRoom();
            if (roomDescription == null)
                return;

            if (options != null && options.Count > 0)
                roomDescription.text = $"How many enemies do you see? ({string.Join("/", options)})";
            else
                roomDescription.text = "How many enemies do you see?";
        }
    }
}

