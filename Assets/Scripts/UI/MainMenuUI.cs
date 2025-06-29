using UnityEngine;
using UnityEngine.UI;
using Evolution.Core;

namespace Evolution.UI
{
    /// <summary>
    /// Displays the main menu buttons and forwards button events.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button highScoresButton;
        [SerializeField] private GameObject setupPanel;
        [SerializeField] private SessionManager sessionManager;

        private void Awake()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OpenSetup);
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(LoadGame);
            if (tutorialButton != null)
                tutorialButton.onClick.AddListener(OpenTutorial);
            if (highScoresButton != null)
                highScoresButton.onClick.AddListener(OpenHighScores);
        }

        private void OnDestroy()
        {
            if (newGameButton != null)
                newGameButton.onClick.RemoveListener(OpenSetup);
            if (loadGameButton != null)
                loadGameButton.onClick.RemoveListener(LoadGame);
            if (tutorialButton != null)
                tutorialButton.onClick.RemoveListener(OpenTutorial);
            if (highScoresButton != null)
                highScoresButton.onClick.RemoveListener(OpenHighScores);
        }

        private void OpenSetup()
        {
            if (setupPanel != null)
                setupPanel.SetActive(true);
        }

        private void LoadGame()
        {
            if (sessionManager != null)
                sessionManager.LoadSession(0);
        }

        private void OpenTutorial()
        {
            // TODO: open tutorial interface
        }

        private void OpenHighScores()
        {
            // TODO: show high scores
        }
    }
}
