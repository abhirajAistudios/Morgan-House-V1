using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button Resume;
    [SerializeField] private Button ExitToMainMenu;
    [SerializeField] private Image pausePanel;



    private bool isPaused = false;
   



 

    void Start()
    {
        // Initially hide the pause panel
        pausePanel.gameObject.SetActive(false);

        // Add listeners to buttons
        Resume.onClick.AddListener(ResumeGame);
       
        ExitToMainMenu.onClick.AddListener(GoToMainMenu);
      
    }

    void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze the game
        pausePanel.gameObject.SetActive(true); // Show pause menu
        // Optional: Unlock cursor if your game normally hides it
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume normal time
        pausePanel.gameObject.SetActive(false); // Hide pause menu
        // Optional: Lock cursor again if needed
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f; // Make sure to reset time scale
        SceneManager.LoadScene("NewGameScene"); // Load the main menu scene
    }


}