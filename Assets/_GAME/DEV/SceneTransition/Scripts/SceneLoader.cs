using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Drag & Drop Scenes Here")]
#if UNITY_EDITOR
    public List<SceneAsset> sceneAssets = new List<SceneAsset>(); // Editor-only scene references
#endif

    [HideInInspector]
    public List<string> sceneNames = new List<string>(); // Actual names used at runtime

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
            // Convert SceneAssets to string names at runtime
            sceneNames.Clear();
            foreach (var sceneAsset in sceneAssets)
            {
                if (sceneAsset != null)
                {
                    sceneNames.Add(sceneAsset.name);
                }
            }
#endif
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
            LoadingManager.Instance.LoadSceneByName(sceneNames[index]);
        }
        else
        {
            Debug.LogError("Invalid scene index: " + index);
        }
    }

    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
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
}
