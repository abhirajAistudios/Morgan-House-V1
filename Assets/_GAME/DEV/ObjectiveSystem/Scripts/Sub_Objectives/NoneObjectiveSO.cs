using UnityEngine;

[CreateAssetMenu(menuName = "Objective System/None Objective")]
public class NoneObjectiveSO : ObjectiveDataSO
{
    public override void Initialize()
    {
        CompleteObjective();
    }
}