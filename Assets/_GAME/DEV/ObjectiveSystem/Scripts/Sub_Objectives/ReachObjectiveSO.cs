using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Objective System/Reach Objective")]
public class ReachObjectiveSO :  ObjectiveDataSO
{
    public Transform TargetPosition;
    public float Threshold = 3f;

    public void CheckReached(Transform playerPos)
    {
        if (objectiveStatus != ObjectiveStatus.INPROGRESS) return;

        if (Vector3.Distance(playerPos.position, TargetPosition.position) < Threshold && AreChildrenComplete())
        {
            CompleteObjective();
        }
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }
}
