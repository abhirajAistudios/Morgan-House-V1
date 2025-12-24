using System.Collections;
using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractables
{
    [SerializeField] protected InteractableData data;
    protected bool hasBeenUsed = false;
    public bool interacted = false;

    public virtual string DisplayName => data != null ? data.displayName : name;
    public virtual string Description => data != null ? data.description : "";
    public virtual bool IsInteractable => data == null || data.isReusable || !hasBeenUsed;

    public virtual void OnFocus() { }
    public virtual void OnLoseFocus() { }

    public virtual void OnInteract()
    {
        if (!IsInteractable) return;
        if (!data.isReusable) hasBeenUsed = true;
    }

    public virtual IEnumerator InteractOnce()
    {
        yield return new WaitForSeconds(3);
    }
    public virtual string GetTooltipText() => DisplayName;
}