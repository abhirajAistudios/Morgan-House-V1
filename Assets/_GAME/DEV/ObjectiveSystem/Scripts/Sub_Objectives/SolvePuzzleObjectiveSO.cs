using UnityEngine;

/// <summary>
/// ScriptableObject that defines a puzzle-solving objective.
/// Completes when the specified puzzle is solved and all child objectives are complete.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Object/Objective System/Solve Puzzle Objective")]
public class SolvePuzzleObjectiveSO : ObjectiveDataSO
{
    [Tooltip("The unique identifier of the puzzle that needs to be solved")]
    public string ItemId;
    
    /// Initializes the objective by subscribing to puzzle solved events
    public override void Initialize()
    {
        // Listen for puzzle solved events to check if this objective is completed
        GameService.Instance.EventService.OnPuzzleSolved.AddListener(OnPuzzleSolved);
    }
    
    /// Called when any puzzle is solved in the game
    public void OnPuzzleSolved(string itemId)
    {
        // Only proceed if this is the correct puzzle and the objective is in progress
        if (itemId != ItemId || objectiveStatus != ObjectiveStatus.INPROGRESS) return;

        // Complete the objective if all child objectives are complete and it's not already completed
        if(AreChildrenComplete() && objectiveStatus != ObjectiveStatus.COMPLETED)
        {
            CompleteObjective();
        }
    }
}