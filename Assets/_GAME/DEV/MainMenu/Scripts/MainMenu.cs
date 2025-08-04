using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;

    [Header("Exit Confirmation Popup")]
    [SerializeField] private GameObject exitConfirmationPanel;
    [SerializeField] private Button yesExitButton;
    [SerializeField] private Button noExitButton;

    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    private void Start()
    {
        // Show Resume only if a save exists
        resumeButton.gameObject.SetActive(File.Exists(savePath));

        // Hook up button listeners
        newGameButton.onClick.AddListener(StartNewGame);
        resumeButton.onClick.AddListener(ResumeGame);
        exitButton.onClick.AddListener(ShowExitPopup);

        yesExitButton.onClick.AddListener(ExitGame);
        noExitButton.onClick.AddListener(HideExitPopup);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false); // hide by default
    }

    private void StartNewGame()
    {
        // Delete save file if exists
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Old save deleted. Starting new game.");
        }

        // Load first scene (replace "GameScene" with your actual scene name)
        SceneManager.LoadScene("Morgan_House");
    }

    private void ResumeGame()
    {
        if (File.Exists(savePath))
        {
            Debug.Log("Resuming game...");
            SceneManager.LoadScene("Morgan_House");
        }
        else
        {
            Debug.LogWarning("No save file found, starting new game instead.");
            StartNewGame();
        }
    }

    private void ShowExitPopup()
    {
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(true);
    }

    private void HideExitPopup()
    {
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);
    }

    private void ExitGame()
    {
        Debug.Log("Exiting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
