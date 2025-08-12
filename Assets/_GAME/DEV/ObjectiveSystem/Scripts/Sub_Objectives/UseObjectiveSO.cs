using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Objective System/Use Objective")]
public class UseObjectiveSO : ObjectiveDataSO
{
    public string ItemId;
    
    public void OnItemUsed(string itemId)
    {
        if (itemId != ItemId || objectiveStatus != ObjectiveStatus.INPROGRESS) return;

        if (AreChildrenComplete())
        {
            CompleteObjective();
        }
    }
    public override void Initialize()
    {
        GameService.Instance.EventService.OnObjectUsed.AddListener(OnItemUsed);
    }
}
