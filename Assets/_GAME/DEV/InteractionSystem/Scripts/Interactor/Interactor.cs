using TMPro;
using UnityEngine;

/// <summary>
/// Handles player interaction using raycasting from the camera.
/// Displays tooltips and manages interaction events.
/// </summary>
public class Interactor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Maximum distance to detect objects.")]
    public float rayDistance = 3f;

    [Tooltip("Distance within which interaction is allowed.")]
    public float interactDistance = 3f;

    [Tooltip("Layers to consider for interaction.")]
    public LayerMask interactLayer;

    [Header("UI References")]
    [Tooltip("Camera used for raycasting.")]
    public Camera cam;

    [Tooltip("UI element for displaying tooltips.")]
    public GameObject tooltipUI;

    public GameObject interactInfoUI;

    [Tooltip("Text component showing interaction info.")]
    public TMP_Text tooltipText;

    private BaseInteractable currentInteractable;
    private bool interacted = false;

    void Update()
    {
        HandleRaycast();
    }

    /// <summary>
    /// Performs a raycast and handles focus/interact logic.
    /// </summary>
    void HandleRaycast()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactLayer))
        {
            BaseInteractable interactable = hit.collider.GetComponent<BaseInteractable>();

            if (interactable != null)
            {
                // Skip showing anything if not interactable
                if (!interactable.IsInteractable)
                {
                    ClearCurrent();
                    return;
                }

                if (interactable != currentInteractable)
                {
                    ClearCurrent();
                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
                }

                // Show tooltip only within range and if still interactable
                if (hit.distance <= interactDistance)
                {
                    UIEnable();
                    tooltipText.text = currentInteractable.GetTooltipText();

                    GameService.Instance.EventService.ShowPressButton.InvokeEvent();

                    if (GameService.Instance.InputHandler.InteractPressed)
                    {
                        interacted = true;
                        UIDisable();
                        currentInteractable.OnInteract();
                        GameService.Instance.EventService.OnInteractionCompletion.InvokeEvent();
                    }
                }
                else
                {
                    UIDisable();
                }
            }
            else
            {
                ClearCurrent();
            }
        }
        else
        {
            ClearCurrent();
        }
    }


    /// <summary>
    /// Clears current focus and resets tooltip.
    /// </summary>
    void ClearCurrent()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
            UIDisable();
            interacted = false;
        }
    }

    void UIEnable()
    {
        tooltipUI.SetActive(true);
        interactInfoUI.SetActive(true);
    }

    void UIDisable()
    {
        tooltipUI.SetActive(false);
        interactInfoUI.SetActive(false);
    }
}
