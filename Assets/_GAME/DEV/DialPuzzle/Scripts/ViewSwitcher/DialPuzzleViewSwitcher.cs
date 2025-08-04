using UnityEngine;

public class DialPuzzleViewSwitcher : MonoBehaviour
{
    [Header("Puzzle References")]
    public Camera mainCamera;      // Assign the global player camera
    public Camera puzzleCamera;    // Assign THIS puzzle�s camera
    public GameObject puzzleUI;    // Assign THIS puzzle�s UI canvas
    public GameObject playerController;

    private bool inPuzzle = false;

    public void EnterPuzzleView()
    {
        if (inPuzzle) return;
        inPuzzle = true;

        puzzleCamera.gameObject.SetActive(true);
        mainCamera.enabled = false;
        puzzleCamera.enabled = true;

        puzzleUI.SetActive(true);
        playerController.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitPuzzleView()
    {
        if (!inPuzzle) return;
        inPuzzle = false;

        puzzleCamera.gameObject.SetActive(false);
        mainCamera.enabled = true;
        puzzleCamera.enabled = false;

        puzzleUI.SetActive(false);
        playerController.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
