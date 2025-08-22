using System;
using UnityEngine;
using UnityEngine.UI;

public class DialPuzzleController : MonoBehaviour, ISaveable
{
    #region Required Variables
    
    [Header("Dial Puzzle Settings")]
    [SerializeField] private string dialPuzzleName;
    [SerializeField] private Dial[] dials;
    [SerializeField] private int[] correctCombination;
    [SerializeField] private int requiredPlacements = 4;

    [Header("UI and Feedback")]
    [SerializeField] private GameObject solvedText;
    [SerializeField] private Button closeButton;

    private int currentPlacements;
    private bool puzzleUnlocked;
    private bool isSolved;

    private DialPuzzleInteractable dialPuzzleInteractable;
    public DialPuzzleViewSwitcher dialPuzzleViewSwitcher;


    [Header("Unique Save ID")]
    [SerializeField] private string uniqueID;
    
    #endregion

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    private void Awake()
    {
        currentPlacements = 0;
        puzzleUnlocked = false;
        isSolved = false;
        
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

            // Use the puzzle's own view switcher
            if (dialPuzzleViewSwitcher != null)
                dialPuzzleViewSwitcher.EnterPuzzleView();
            else
                Debug.LogWarning($"No DialPuzzleViewSwitcher set on {name}");
        }
    }
    
    /// Enables the first dial in the puzzle, allowing the user to interact with it.
    void EnableFirstDial()
    {
        // Set the flag to indicate the puzzle is unlocked
        puzzleUnlocked = true;

        if (dials.Length > 0)
        {
            // Enable the first dial
            dials[0].SetInteractable(true);
        }
        else
        {
            // If no dials are assigned, log a warning
            Debug.LogWarning("No dials assigned to puzzle controller.");
        }
    }
    
    /// Checks the solution of the dial puzzle.
    public void CheckSolution()
    {
        if (!puzzleUnlocked || isSolved) return;

        // Check each dial's current index against the correct combination
        for (int i = 0; i < dials.Length; i++)
        {
            if (dials[i].CurrentIndex == correctCombination[i])
            {
                // If the dial is correct, set its color to black and disable interaction
                dials[i].GetComponent<Renderer>().material.color = Color.black;
                dials[i].SetInteractable(false);

                // If this is not the last dial, enable the next dial
                if (i < dials.Length - 1)
                    dials[i + 1].SetInteractable(true);
            }
            else
            {
                // If the dial is incorrect, print an error message and stop checking
                return;
            }
        }
        PuzzleSolved();
    }
    
    /// Called when the puzzle is solved.
    void PuzzleSolved()
    {
        isSolved = true;
        
        dialPuzzleInteractable?.MarkSolved();                                                               // Mark the puzzle as solved in the puzzle interactable
        solvedText?.SetActive(true);                                                                        // Show the solved text
        GameService.Instance.EventService.OnPuzzleSolved.InvokeEvent(dialPuzzleName);                       // Notify the game service and event service that the puzzle is solved
        dialPuzzleViewSwitcher.ExitPuzzleView();                                                            // Exit the puzzle view
        closeButton?.gameObject.SetActive(true);                                                            // Enable the close button
        GameProgressTracker.ObjectivesCompleted++;                                                          // Increment the number of objectives completed

        // Save the game after the objective is completed
        Transform playerPos = FindAnyObjectByType<PlayerController>().transform;
        FindAnyObjectByType<AutoSaveManager>()?.SaveAfterObjective(playerPos);
    }
    
    // Saveable Implementation
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
    
    /// Restores the puzzle to its solved state.
    private void RestoreSolvedState()
    {
        isSolved = true;
        puzzleUnlocked = true;
        currentPlacements = requiredPlacements;

        // Disable all dials and turn them black
        foreach (var dial in dials)
        {
            dial.SetInteractable(false);
            dial.GetComponent<Renderer>().material.color = Color.black;
        }

        // Show the solved text and enable the close button
        solvedText?.SetActive(true);
        closeButton?.gameObject.SetActive(true);

        // Mark the puzzle as solved on the interactable component
        dialPuzzleInteractable?.MarkSolved();
    }
    
    /// Restores the puzzle to its unsolved state.
    private void RestoreUnsolvedState()
    {
        // If all objects have been placed, enable the first dial
        if (currentPlacements >= requiredPlacements)
        {
            EnableFirstDial();
        }
    }
}