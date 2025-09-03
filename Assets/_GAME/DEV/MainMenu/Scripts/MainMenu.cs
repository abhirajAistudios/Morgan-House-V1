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

    [Header("New Game Confirmation Popup")]
    [SerializeField] private GameObject newGameConfirmationPanel;
    [SerializeField] private Button yesNewGameButton;
    [SerializeField] private Button noNewGameButton;
    
    [Header("Sound")]
    [SerializeField] private Sounds mainMenuBGM;

    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    private void Start()
    {
        SoundService.Instance.PlayMusic(mainMenuBGM);
        
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(File.Exists(savePath));

        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGamePressed);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(ShowExitPopup);

        if (yesExitButton != null) yesExitButton.onClick.AddListener(ExitGame);
        if (noExitButton != null) noExitButton.onClick.AddListener(HideExitPopup);

        if (yesNewGameButton != null) yesNewGameButton.onClick.AddListener(StartNewGame);
        if (noNewGameButton != null) noNewGameButton.onClick.AddListener(HideNewGamePopup);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        if (newGameConfirmationPanel != null)
            newGameConfirmationPanel.SetActive(false);
    }

    private void OnNewGamePressed()
    {
        if (File.Exists(savePath))
        {
            // Show confirmation popup if save exists
            if (newGameConfirmationPanel != null)
                newGameConfirmationPanel.SetActive(true);
        }
        else
        {
            // Directly start new game if no save
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);

        if (newGameConfirmationPanel != null)
            newGameConfirmationPanel.SetActive(false);

        GameManager.Instance?.StartNewGame();
        SceneLoader.Instance.LoadSceneByIndex(1);
    }

    private void ResumeGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data != null && !string.IsNullOrEmpty(data.lastSceneName))
            {
                Debug.Log("Resuming last saved scene: " + data.lastSceneName);

                GameManager.Instance.ResumeGame();

                if (LoadingManager.Instance != null)
                {
                    LoadingManager.ResumeRequested = true;
                    SceneLoader.Instance.LoadSceneByIndex(data.sceneIndex);
                }
                else
                    Debug.LogError("LoadingManager not found!");
            }
            else
            {
                Debug.LogWarning("No scene name found in save, starting new game.");
                StartNewGame();
            }
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

    private void HideNewGamePopup()
    {
        if (newGameConfirmationPanel != null)
            newGameConfirmationPanel.SetActive(false);
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
