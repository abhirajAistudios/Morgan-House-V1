using UnityEngine;

/// ScriptableObject that defines a reach objective where the player needs to reach a specific position.
[CreateAssetMenu(menuName = "Objective System/Reach Objective")]
public class ReachObjectiveSO : ObjectiveDataSO
{
    [Tooltip("The target position the player needs to reach")]
    public Transform TargetPosition;
    
    [Tooltip("The maximum distance from the target position to consider the objective complete")]
    public float Threshold = 3f;
    
    /// Checks if the player has reached the target position and completes the objective if conditions are met
    public void CheckReached(Transform playerPos)
    {
        // Only proceed if the objective is currently in progress
        if (objectiveStatus != ObjectiveStatus.INPROGRESS) return;

        // Check if player is within threshold distance of target and all child objectives are complete
        if (Vector3.Distance(playerPos.position, TargetPosition.position) < Threshold && AreChildrenComplete())
        {
            CompleteObjective();
        }
    }
    
    /// Initializes the objective by subscribing to the player movement event
    public override void Initialize()
    {
        // Listen for player movement to check if the objective is completed
        GameService.Instance.EventService.OnPlayerMoved.AddListener(CheckReached);
    }
}