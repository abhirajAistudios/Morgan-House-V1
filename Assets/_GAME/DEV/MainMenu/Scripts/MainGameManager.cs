using UnityEngine;

public class MainGameManager : MonoBehaviour
{
     public static MainGameManager Instance;

    public bool isNewGame = false;  // <-- Add this flag

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGame()
    {
        isNewGame = true;
    }

    public void ResumeGame()
    {
        isNewGame = false;
    }
}
