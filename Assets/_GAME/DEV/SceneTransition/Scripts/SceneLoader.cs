using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Enter Scene Names Here")]
    public List<string> sceneNames = new List<string>(); // Scene names used at runtime

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < sceneNames.Count)
        {
            string sceneName = sceneNames[index];
            if (!string.IsNullOrEmpty(sceneName))
            {
                LoadingManager.Instance.LoadSceneByName(sceneName);
            }
            else
            {
                Debug.LogError("Scene name is empty at index: " + index);
            }
        }
        else
        {
            Debug.LogError("Invalid scene index: " + index);
        }
    }

    public void LoadNextScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentIndex = sceneNames.IndexOf(currentSceneName);

        if (currentIndex >= 0)
        {
            int nextIndex = currentIndex + 1;
            if (nextIndex < sceneNames.Count)
            {
                LoadSceneByIndex(nextIndex);
            }
            else
            {
                Debug.Log("No more scenes in list!");
            }
        }
        else
        {
            Debug.LogWarning("Current scene not found in sceneNames list. Loading first scene as fallback.");
            if (sceneNames.Count > 0)
            {
                LoadSceneByIndex(0);
            }
        }
    }
}