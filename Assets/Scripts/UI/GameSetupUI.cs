using UnityEngine;
using UnityEngine.UI;
using Evolution.Core;
using Evolution.Core.Multiplayer;

namespace Evolution.UI
{
    /// <summary>
    /// Handles the new game setup panel allowing players to choose
    /// difficulty and multiplayer mode.
    /// </summary>
    public class GameSetupUI : MonoBehaviour
    {
        [SerializeField] private Dropdown difficultyDropdown;
        [SerializeField] private Dropdown gameTypeDropdown;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private LobbyManager lobbyManager;

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirm);
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancel);
        }

        private void OnConfirm()
        {
            if (gameManager == null)
                return;

            if (difficultyDropdown != null)
            {
                string diff = difficultyDropdown.options[difficultyDropdown.value].text;
                gameManager.Difficulty = diff;
            }

            GameType type = GameType.Solo;
            if (gameTypeDropdown != null)
                type = (GameType)gameTypeDropdown.value;
            gameManager.GameType = type;

            int ownerId = 0; // local player id placeholder
            if (type == GameType.Solo)
            {
                gameManager.StartNewGame(ownerId);
            }
            else
            {
                lobbyManager?.CreateLobby("Lobby" + ownerId, ownerId, type, gameManager.Difficulty, null);
                gameManager.StartNewGame(ownerId);
            }

            gameObject.SetActive(false);
        }

        private void OnCancel()
        {
            gameObject.SetActive(false);
        }
    }
}
