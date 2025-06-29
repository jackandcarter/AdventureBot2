using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Evolution.Core;
using Evolution.Data;

namespace Evolution.UI
{
    /// <summary>
    /// Displays a dropdown of available classes and shows stats for the
    /// selected entry. When confirmed the chosen class is assigned to the
    /// GameManager and the player is built with that class.
    /// </summary>
    public class ClassSelectUI : MonoBehaviour
    {
        [SerializeField] private DataManager dataManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Dropdown classDropdown;
        [SerializeField] private Image classImage;
        [SerializeField] private Text statsText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private List<Sprite> classSprites = new();

        private List<PlayerClass> classes;

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(Confirm);
            if (classDropdown != null)
                classDropdown.onValueChanged.AddListener(_ => UpdateDisplay());
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(Confirm);
            if (classDropdown != null)
                classDropdown.onValueChanged.RemoveListener(_ => UpdateDisplay());
        }

        private void OnEnable()
        {
            PopulateDropdown();
            UpdateDisplay();
        }

        private void PopulateDropdown()
        {
            classes = dataManager != null ? dataManager.GetClassDatabase()?.Classes : null;
            if (classDropdown == null || classes == null) return;

            classDropdown.ClearOptions();
            classDropdown.AddOptions(classes.Select(c => c.ClassName).ToList());
        }

        private void UpdateDisplay()
        {
            if (classes == null || classes.Count == 0 || classDropdown == null)
                return;
            int index = Mathf.Clamp(classDropdown.value, 0, classes.Count - 1);
            var cls = classes[index];

            if (classImage != null && index < classSprites.Count)
                classImage.sprite = classSprites[index];

            if (statsText != null)
            {
                var lines = cls.Stats
                    .Where(s => s != null && s.Stat != null)
                    .Select(s => $"{s.Stat.Name}: {s.Value}");
                statsText.text = string.Join("\n", lines);
            }
        }

        private void Confirm()
        {
            if (classes == null || classDropdown == null || gameManager == null)
                return;
            int index = Mathf.Clamp(classDropdown.value, 0, classes.Count - 1);
            var cls = classes[index];
            gameManager.SelectedClass = cls;

            int owner = NetworkManager.Singleton != null ? (int)NetworkManager.Singleton.LocalClientId : 0;
            gameManager.BuildPlayer(owner, cls);

            gameObject.SetActive(false);
        }
    }
}
