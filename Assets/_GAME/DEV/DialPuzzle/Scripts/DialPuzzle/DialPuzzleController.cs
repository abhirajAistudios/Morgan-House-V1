using System;
using UnityEngine;
using UnityEngine.UI;

public class DialPuzzleController : MonoBehaviour, ISaveable
{
    [Header("Dial Puzzle Settings")]
    public string DialPuzzleName;
    public Dial[] dials;
    public int[] correctCombination;
    public int requiredPlacements = 4;

    [Header("UI and Feedback")]
    public GameObject solvedText;
    public Button CloseButton;

    private int currentPlacements = 0;
    private bool puzzleUnlocked = false;
    private bool isSolved = false;

    private DialPuzzleInteractable dialPuzzleInteractable;
    [SerializeField] private DialPuzzleViewSwitcher dialPuzzleViewSwitcher;


    [Header("Unique Save ID")]
    [SerializeField] private string uniqueID;

    public Transform player;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    private void Awake()
    {
        if (dialPuzzleInteractable == null)
            dialPuzzleInteractable = GetComponentInChildren<DialPuzzleInteractable>();

        if (dialPuzzleViewSwitcher == null)
            dialPuzzleViewSwitcher = GetComponentInChildren<DialPuzzleViewSwitcher>();
    }

    private void Start()
    {
        if (dialPuzzleInteractable != null)
            dialPuzzleInteractable.isSolved = true;
    }

    public void RegisterPlacement()
    {
        currentPlacements++;

        if (currentPlacements >= requiredPlacements && !puzzleUnlocked)
        {
            dialPuzzleInteractable.isSolved = false;
            EnableFirstDial();

            // Use the puzzle�s own view switcher
            if (dialPuzzleViewSwitcher != null)
                dialPuzzleViewSwitcher.EnterPuzzleView();
            else
                Debug.LogWarning($"No DialPuzzleViewSwitcher set on {name}");
        }
    }


    void EnableFirstDial()
    {
        puzzleUnlocked = true;

        if (dials.Length > 0)
        {
            dials[0].SetInteractable(true);
            Debug.Log("Dial puzzle unlocked: First dial activated.");
        }
        else
        {
            Debug.LogWarning("No dials assigned to puzzle controller.");
        }
    }

    public void CheckSolution()
    {
        if (!puzzleUnlocked || isSolved) return;

        for (int i = 0; i < dials.Length; i++)
        {
            if (dials[i].CurrentIndex == correctCombination[i])
            {
                dials[i].GetComponent<Renderer>().material.color = Color.black;
                dials[i].SetInteractable(false);

                if (i < dials.Length - 1)
                    dials[i + 1].SetInteractable(true);
            }
            else
            {
                Debug.Log($"Dial {i + 1} incorrect. Got {dials[i].CurrentIndex}, expected {correctCombination[i]}");
                return;
            }
        }

        PuzzleSolved();
    }

    void PuzzleSolved()
    {
        Debug.Log("Dial Puzzle Solved!");
        isSolved = true;

        dialPuzzleInteractable?.MarkSolved();
        solvedText?.SetActive(true);
        GameService.Instance.EventService.OnPuzzleSolved.InvokeEvent(DialPuzzleName);
        dialPuzzleViewSwitcher.ExitPuzzleView();
        CloseButton?.gameObject.SetActive(true);

        GameProgressTracker.ObjectivesCompleted++;
        FindAnyObjectByType<AutoSaveManager>()?.SaveAfterObjective(player);
    }

    // -----------------------
    // ISaveable Implementation
    // -----------------------
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        AutoSaveManager.PuzzleState state = new AutoSaveManager.PuzzleState
        {
            puzzleID = uniqueID,
            isSolved = isSolved
        };

        data.puzzles.Add(state);
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        foreach (var state in data.puzzles)
        {
            if (state.puzzleID == uniqueID)
            {
                if (state.isSolved)
                    RestoreSolvedState();
                else
                    RestoreUnsolvedState();
                return;
            }
        }
    }

    private void RestoreSolvedState()
    {
        isSolved = true;
        puzzleUnlocked = true;

        foreach (var dial in dials)
        {
            dial.SetInteractable(false);
            dial.GetComponent<Renderer>().material.color = Color.black;
        }

        solvedText?.SetActive(true);
        CloseButton?.gameObject.SetActive(true);
        dialPuzzleInteractable?.MarkSolved();

        Debug.Log($"Dial puzzle [{uniqueID}] restored as solved.");
    }

    private void RestoreUnsolvedState()
    {
        if (currentPlacements >= requiredPlacements)
        {
            EnableFirstDial();
        }
        Debug.Log($"Dial puzzle [{uniqueID}] restored as unsolved.");
    }
}
