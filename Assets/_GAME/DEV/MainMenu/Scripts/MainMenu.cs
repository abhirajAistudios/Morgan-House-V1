using UnityEngine;
using UnityEngine.UI;
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
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(File.Exists(savePath));

        if (newGameButton != null) newGameButton.onClick.AddListener(StartNewGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(ShowExitPopup);

        if (yesExitButton != null) yesExitButton.onClick.AddListener(ExitGame);
        if (noExitButton != null) noExitButton.onClick.AddListener(HideExitPopup);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);
    }

    private void StartNewGame()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);

        GameManager.Instance?.StartNewGame();

        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadSceneByName("Morgan_House");
        else
            Debug.LogError("LoadingManager not found!");
    }

    private void ResumeGame()
    {
        if (File.Exists(savePath))
        {
            LoadingManager.Instance.LoadSceneByName("Morgan_House");
            GameManager.Instance.ResumeGame();

            if (LoadingManager.Instance != null)
            {

               
            }
               
                 
            else
                Debug.LogError("LoadingManager not found!");
        }
        else
        {
            Debug.LogWarning("No save file, starting new game.");
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
