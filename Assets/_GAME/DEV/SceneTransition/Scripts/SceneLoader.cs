using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Singleton for easy access
    public static SceneLoader Instance;

    [Header("Scene List (add scenes from Build Settings here)")]
    public List<string> sceneNames = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keeps this across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ✅ Load scene by index (position in the list)
    public void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < sceneNames.Count)
        {
            LoadingManager.Instance.LoadSceneByName(sceneNames[index]);
        }
        else
        {
            Debug.LogError("Invalid scene index: " + index);
        }
    }
    

    // ✅ Load next scene in the list
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex <= sceneNames.Count)
        {
            LoadSceneByIndex(nextIndex);
        }
        else
        {
            Debug.Log("No more scenes in list!");
        }
    }
}
