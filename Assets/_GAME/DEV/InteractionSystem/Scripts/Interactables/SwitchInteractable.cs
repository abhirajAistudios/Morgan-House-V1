using UnityEngine;

/// <summary>
/// Represents a switch that can be used by the player. It highlights on focus,
/// changes color on interaction, and plays a sound.
/// </summary>
public class SwitchInteractable : BaseInteractable
{
    [Header("UI Info")]
    [Tooltip("Tooltip text when aiming at the object.")]
    [SerializeField] private string tooltip = "Use";

    [Tooltip("Display name shown in tooltip and UI.")]
    [SerializeField] private string displayName = "Switch";

    [Tooltip("Optional description shown in viewer or logs.")]
    [TextArea(5, 10)]
    [SerializeField] private string description = "A mechanical switch. Might activate something nearby.";

    [Header("Visuals")]
    [SerializeField] private Color focusColor = Color.yellow;
    [SerializeField] private Color unfocusColor = Color.cyan;
    [SerializeField] private Color usedColor = Color.green;

    // Exposed to other systems (UI, viewer, etc.)
    public override string DisplayName => displayName;
    public override string Description => description;
    public override string GetTooltipText() => !string.IsNullOrEmpty(tooltip) ? tooltip : displayName;

    public override void OnFocus()
    {
        base.OnFocus();

        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = focusColor;
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = unfocusColor;
        }
    }

    public override void OnInteract()
    {
        base.OnInteract(); // sets hasBeenUsed if isReusable == false

        if (!IsInteractable)
        {
            Debug.Log($"[{name}] already used and not reusable.");
            return;
        }

        // Play audio
        GameService.Instance.SoundService?.Play(Sounds.SWITCH);

        // Visual feedback
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = usedColor;
        }

        // Optional: trigger events, animations, logic here
        Debug.Log($"Switch [{name}] was used.");
    }
}
