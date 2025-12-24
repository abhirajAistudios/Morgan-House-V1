using UnityEngine;

/// <summary>
/// Central event management system for handling game-wide events using the observer pattern.
/// Provides strongly-typed event controllers for different game events.
/// </summary>
public class EventService : GenericMonoSingleton<EventService>
{
    //Triggered when a UI button should show press feedback
    public EventController ShowPressButton { get; private set; }
    
    //Triggered when any interaction is completed
    public EventController OnInteractionCompletion { get; private set; }

    //Triggered when an object is collected, passing the object's ID
    public EventController<string> OnObjectCollected { get; private set; }
    
    //Triggered when a puzzle is solved, passing the puzzle ID
    public EventController<string> OnPuzzleSolved { get; private set; }
    
    //Triggered when an object is used, passing the object's ID
    public EventController<string> OnObjectUsed { get; private set; }
 
    //Triggered when an objective is completed, passing the objective data
    public EventController<ObjectiveDataSO> OnObjectiveCompleted { get; private set; }

    //Triggered when the player moves, passing the player's transform for ReachObjective
    public EventController<Transform> OnPlayerMoved { get; private set; }
    
    /// Initializes all event controllers
    public EventService()
    {
        // Initialize all event controllers
        ShowPressButton = new EventController();
        OnInteractionCompletion = new EventController();
        OnObjectCollected = new EventController<string>();
        OnPuzzleSolved = new EventController<string>();
        OnObjectUsed = new EventController<string>();
        OnObjectiveCompleted = new EventController<ObjectiveDataSO>();
        OnPlayerMoved = new EventController<Transform>();
    }
}