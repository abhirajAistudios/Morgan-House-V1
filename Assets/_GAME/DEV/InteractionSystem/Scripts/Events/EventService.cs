public class EventService
{
    public EventController ShowPressButton { get; private set; }
    public EventController OnInteractionCompletion { get; private set; }
    public EventController<string> OnObjectCollected { get; private set; }
    public EventController<string> OnPuzzleSolved { get; private set; }

    public EventController<string> OnObjectUsed { get; private set; }
    
    public EventController<ObjectiveDataSO>  OnObjectiveCompleted { get; private set; }
    public EventService()
    {
        ShowPressButton = new EventController();
        OnInteractionCompletion = new EventController();
        OnObjectCollected = new EventController<string>();
        OnPuzzleSolved = new EventController<string>();
        OnObjectUsed = new EventController<string>();
        OnObjectiveCompleted = new EventController<ObjectiveDataSO>();
    }
}