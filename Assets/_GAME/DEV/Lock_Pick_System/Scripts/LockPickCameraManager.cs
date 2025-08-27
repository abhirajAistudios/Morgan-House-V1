using UnityEngine;

public class LockPickCameraManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera lockpickCamera;
    [SerializeField] private GameObject player;

    [Header("Lockpick Objects")]
    [SerializeField] private GameObject[] lockpickObjects;

    private bool isLockpicking = false;
    private GameObject objectiveCanvas;

    private void Start()
    {
        objectiveCanvas = GameObject.FindWithTag("Objective Canvas");
    }
    // Call this to start lockpicking
    public void EnterLockpickMode()
    {
        isLockpicking = true;
        
        objectiveCanvas.SetActive(false);
        lockpickCamera.gameObject.SetActive(true);
        lockpickCamera.enabled = true;
        mainCamera.enabled = false;
        
        player.SetActive(false);

        foreach (GameObject obj in lockpickObjects)
            obj.SetActive(true);
    }

    // Call this when lockpicking is done successfully
    public void ExitLockpickMode()
    {
        isLockpicking = false;
        
        objectiveCanvas.SetActive(true);
        lockpickCamera.gameObject.SetActive(false);
        lockpickCamera.enabled = false;
        mainCamera.enabled = true;
        player.SetActive(true);

        foreach (GameObject obj in lockpickObjects)
            obj.SetActive(false);
    }
}