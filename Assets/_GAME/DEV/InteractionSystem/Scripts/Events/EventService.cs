public class EventService
{
    public EventController ShowPressButton { get; private set; }
    public EventController OnInteractionCompletion { get; private set; }
    public EventController<string> OnPuzzleObjectCollected { get; private set; }
    public EventController<string> OnPuzzleSolved { get; private set; }

    public EventController<string> OnObjectUsed { get; private set; }
    public EventService()
    {
        ShowPressButton = new EventController();
        OnInteractionCompletion = new EventController();
        OnPuzzleObjectCollected = new EventController<string>();
        OnPuzzleSolved = new EventController<string>();
        OnObjectUsed = new EventController<string>();
    }
}