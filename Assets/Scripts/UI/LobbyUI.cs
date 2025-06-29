using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Evolution.Core.Multiplayer;

namespace Evolution.UI
{
    /// <summary>
    /// Simple lobby browser and host controls.
    /// Displays available lobbies and lets the user create or join sessions.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private LobbyManager lobbyManager;
        [SerializeField] private GameObject lobbyItemPrefab;
        [SerializeField] private Transform lobbyListRoot;
        [SerializeField] private InputField lobbyNameInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button startButton;
        [SerializeField] private string gameplayScene = "SampleScene";
        [SerializeField] private ClassSelectUI classSelectPanel;

        private Lobby currentLobby;

        private void Awake()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(Refresh);
            if (createButton != null)
                createButton.onClick.AddListener(CreateLobby);
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);
        }

        private void OnDestroy()
        {
            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(Refresh);
            if (createButton != null)
                createButton.onClick.RemoveListener(CreateLobby);
            if (startButton != null)
                startButton.onClick.RemoveListener(StartGame);
        }

        private void ClearList()
        {
            if (lobbyListRoot == null) return;
            foreach (Transform child in lobbyListRoot)
                Destroy(child.gameObject);
        }

        /// <summary>
        /// Refresh lobby listing from LobbyManager.
        /// </summary>
        public void Refresh()
        {
            if (lobbyManager == null || lobbyItemPrefab == null || lobbyListRoot == null)
                return;

            ClearList();
            var lobbies = lobbyManager.ListLobbies();
            foreach (var lobby in lobbies)
            {
                var item = Instantiate(lobbyItemPrefab, lobbyListRoot);
                var text = item.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = $"{lobby.Name} ({lobby.Players.Count}/8)";
                var btn = item.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    int id = lobby.LobbyId;
                    btn.onClick.AddListener(() => JoinLobby(id));
                }
            }
        }

        private void CreateLobby()
        {
            if (lobbyManager == null)
                return;

            string name = lobbyNameInput != null ? lobbyNameInput.text : $"Lobby{Random.Range(1000,9999)}";
            string pwd = passwordInput != null ? passwordInput.text : null;
            currentLobby = lobbyManager.CreateLobby(name, (int)NetworkManager.Singleton.LocalClientId, GameType.Multiplayer, "Easy", pwd);
            Refresh();
            if (classSelectPanel != null)
                classSelectPanel.gameObject.SetActive(true);
        }

        private void JoinLobby(int lobbyId)
        {
            if (lobbyManager == null)
                return;
            string pwd = passwordInput != null ? passwordInput.text : null;
            if (lobbyManager.JoinLobby(lobbyId, NetworkManager.Singleton.LocalClientId, pwd))
            {
                currentLobby = lobbyManager.ListLobbies().FirstOrDefault(l => l.LobbyId == lobbyId);
                if (classSelectPanel != null)
                    classSelectPanel.gameObject.SetActive(true);
            }
        }

        private void StartGame()
        {
            if (currentLobby == null)
                return;
            if (NetworkManager.Singleton == null)
                return;
            if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
                return;

            NetworkManager.Singleton.SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
        }
    }
}
