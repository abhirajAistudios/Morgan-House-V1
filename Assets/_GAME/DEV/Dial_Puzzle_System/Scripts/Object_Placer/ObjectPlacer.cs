using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the logic for snapping puzzle objects into predefined slots and notifying the puzzle controller.
/// Prevents duplicate placements and optionally locks the objects after all slots are filled.
/// </summary>
public class ObjectPlacer : MonoBehaviour
{
    private bool alignRotation = true;             // If true, objects will match the slot's rotation when snapped
    private bool freezeAfterSnap = true;           // If true, rigidbody is frozen after snap to prevent movement

    private List<GameObject> placedObjects = new List<GameObject>(); // Track all placed objects
    private bool placementLocked = false;         // Prevents more placements after all slots are filled


    /// <summary>
    /// Call this to place an object into the next available slot.
    /// Snaps it into position, aligns rotation if enabled, and notifies the puzzle controller.
    /// </summary>
    public void AddThisObjectInHolder(GameObject puzzleObject, DialPuzzleController dialPuzzleController)
    {
        if (puzzleObject == null || placementLocked)
            return;

        if (placedObjects.Contains(puzzleObject))
        {
            Debug.Log("⚠️ Object already placed.");
            return;
        }

        // Optional: Disable physics so the object stays in place
        if (freezeAfterSnap && puzzleObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        placedObjects.Add(puzzleObject);

        // Notify the puzzle that a placement has been made
        dialPuzzleController?.RegisterPlacement();
    }
}