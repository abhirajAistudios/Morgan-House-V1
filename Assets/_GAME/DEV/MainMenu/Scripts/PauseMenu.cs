using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button Resume;
    [SerializeField] private Button ExitToMainMenu;
    [SerializeField] private GameObject pausePanel;

    [Header("Sounds")]
    [SerializeField] private Sounds pauseMenuBGM;
    [SerializeField] private Sounds ingameBGM;
    private bool isPaused = false;

    void Start()
    {
        SoundService.Instance.DisableAudioListener();
        SoundService.Instance.PlayMusic(ingameBGM);
        // Initially hide the pause panel
        pausePanel.gameObject.SetActive(false);

        // Add listeners to buttons
        Resume.onClick.AddListener(ResumeGame);
        ExitToMainMenu.onClick.AddListener(GoToMainMenuWithLoading);
    }

    void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze the game
        SoundService.Instance.EnableAudioListener();
        SoundService.Instance.PlayMusic(pauseMenuBGM);
        pausePanel.gameObject.SetActive(true); // Show pause menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        isPaused = false;
        SoundService.Instance.DisableAudioListener();
        SoundService.Instance.PlayMusic(ingameBGM);
        Time.timeScale = 1f; // Resume normal time
        pausePanel.gameObject.SetActive(false); // Hide pause menu
    }

    //  Uses LoadingManager instead of SceneManager directly
    private void GoToMainMenuWithLoading()
    {
        Time.timeScale = 1f; // Reset time scale

        if (LoadingManager.Instance != null)
        {
            Time.timeScale = 0f;
            gameObject.SetActive(false);
            SceneLoader.Instance.LoadSceneByIndex(0);
        }
        else
        {
            Debug.LogError("LoadingManager instance not found! Falling back to direct SceneManager.");
            SceneManager.LoadScene(0);
        }
    }
}
