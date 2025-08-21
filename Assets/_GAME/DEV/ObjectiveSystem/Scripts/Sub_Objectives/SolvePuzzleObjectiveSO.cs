using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Objective System/Solve Puzzle Objective")]
public class SolvePuzzleObjectiveSO : ObjectiveDataSO
{
    public string ItemId;
    
    public override void Initialize()
    {
        GameService.Instance.EventService.OnPuzzleSolved.AddListener(OnPuzzleSolved);
    }
    
    public void OnPuzzleSolved(string itemId)
    {
        if (itemId != ItemId || objectiveStatus != ObjectiveStatus.INPROGRESS) return;

        if(AreChildrenComplete()  && objectiveStatus != ObjectiveStatus.COMPLETED)
        {
            CompleteObjective();
        }
    }
}