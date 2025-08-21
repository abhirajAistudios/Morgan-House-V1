using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Objective System/None Objective")]
public class NoneObjectiveSO : ObjectiveDataSO
{
    public override void Initialize()
    {
        CompleteObjective();
    }
}