using UnityEngine;

public class LockPickCameraManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera lockpickCamera;
   // public Camera DialPuzzleCamera;
    public GameObject player;

    [Header("Lockpick Objects")]
    public GameObject[] lockpickObjects;

    private bool isLockpicking = false;

    // Call this to start lockpicking
    public void EnterLockpickMode()
    {
        isLockpicking = true;

        lockpickCamera.gameObject.SetActive(true);
        lockpickCamera.enabled = true;
        mainCamera.enabled = false;
        
//        DialPuzzleCamera.enabled = false;
        player.SetActive(false);
        

        foreach (GameObject obj in lockpickObjects)
            obj.SetActive(true);
    }

    // Call this when lockpicking is done successfully
    public void ExitLockpickMode()
    {
        isLockpicking = false;

        lockpickCamera.gameObject.SetActive(false);
        lockpickCamera.enabled = false;
        mainCamera.enabled = true;
    //    DialPuzzleCamera.enabled = true;
        player.SetActive(true);

        foreach (GameObject obj in lockpickObjects)
            obj.SetActive(false);
    }
}
