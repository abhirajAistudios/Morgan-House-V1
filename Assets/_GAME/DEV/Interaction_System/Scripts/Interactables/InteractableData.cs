using UnityEngine;

[CreateAssetMenu(fileName = "InteractableData", menuName = "Interaction/Interactable Data")]
public class InteractableData : ScriptableObject
{
    public string displayName = "Unnamed Object";
    [TextArea(3, 5)] public string description = "No description provided.";
    public bool isReusable = true;  // true for letters/photos, false for puzzles
}