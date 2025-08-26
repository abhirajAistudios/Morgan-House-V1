using ExpressElevator.Utilities;
using UnityEngine;

public class GameService : GenericMonoSingleton<GameService>
{
    public EventService EventService { get; private set; }
    public InputHandler InputHandler;
    public ObjectViewer ObjectViewer;
    public ObjectPlacer ObjectPlacer;
    
    [SerializeField]private UIService _uiService;
    public UIService UIService => _uiService;
    
    private void Start()
    {
        InitializeService();
        InitializeDependencies();
    }
    private void InitializeService()
    {
        EventService = new EventService();
    }
    private void InitializeDependencies()
    {
        _uiService.InitializeDependencies(EventService);
    }
}