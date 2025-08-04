using TMPro;
using UnityEngine;

/// <summary>
/// Displays a 3D object viewer with smooth zoom, rotation clamping,
/// and idle auto-rotation when not interacting.
/// </summary>
public class ObjectViewer : MonoBehaviour
{
    [Header("References")]
    public Transform pivot;
    public Camera viewerCamera;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Interaction Settings")]
    public float rotationSpeed = 5f;
    public float zoomSpeed = 3f;
    public float minZoom = 2f;
    public float maxZoom = 8f;
    public LayerMask objectLayer = 0;

    [Header("Auto-Rotate")]
    public float idleRotateSpeed = 20f;
    public float idleDelay = 3f; // seconds after last interaction

    private GameObject currentObject;
    private IInteractables _currentInteractor;
    private Vector3 lastMousePos;
    private float targetDistance;
    private float currentDistance;
    private float lastInteractionTime;

    private float xRotation = 0f; // for clamping X-axis rotation

    public void Show(GameObject prefab, IInteractables interactables)
    {
        if (currentObject) Destroy(currentObject);

        currentObject = Instantiate(prefab, pivot);
        Debug.Log("Instantiated: " + currentObject.name);

        _currentInteractor = interactables;

        currentObject.transform.localPosition = Vector3.zero;
        currentObject.transform.localRotation = Quaternion.identity;

        // Set object and children layers
        int layer = (objectLayer.value != 0)
            ? Mathf.RoundToInt(Mathf.Log(objectLayer.value, 2))
            : LayerMask.NameToLayer("Default");

        SetLayerRecursively(currentObject, layer);

        // Calculate bounds for scale and camera distance
        Bounds bounds = CalculateBounds(currentObject);
        float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float scaleFactor = Mathf.Clamp(1f / size, 0.1f, 10f);
        currentObject.transform.localScale = Vector3.one * scaleFactor;

        // Position camera
        targetDistance = Mathf.Clamp(size * 2f, minZoom, maxZoom);
        currentDistance = targetDistance;
        viewerCamera.transform.position = pivot.position - viewerCamera.transform.forward * currentDistance;
        viewerCamera.transform.LookAt(pivot);

        // Update UI
        GetObjectTitle();
        GetObjectDescription();

        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        lastInteractionTime = Time.time;
    }

    public void Close()
    {
        if (currentObject) Destroy(currentObject);
        gameObject.SetActive(false);
        FindObjectOfType<ObjectViewSwitcher>().ExitPuzzleView();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        bool interacted = false;

        // --- Mouse Rotation ---
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            float xDelta = -delta.x * rotationSpeed;
            float yDelta = delta.y * rotationSpeed;

            pivot.Rotate(Vector3.up, xDelta, Space.World); // Y-axis (global)
            xRotation = Mathf.Clamp(xRotation + yDelta, -80f, 80f);
            pivot.localRotation = Quaternion.Euler(xRotation, pivot.localEulerAngles.y, 0f); // X-axis (local)

            interacted = true;
        }

        // --- Smooth Zoom ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
            interacted = true;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 5f);
        viewerCamera.transform.position = pivot.position - viewerCamera.transform.forward * currentDistance;

        // --- Auto-Rotate When Idle ---
        if (!interacted && Time.time - lastInteractionTime > idleDelay)
        {
            pivot.Rotate(Vector3.up, idleRotateSpeed * Time.deltaTime, Space.World);
        }

        if (interacted)
        {
            lastInteractionTime = Time.time;
        }

        lastMousePos = Input.mousePosition;

        // --- Close Viewer ---
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found on object.");
            return new Bounds(go.transform.position, Vector3.one);
        }

        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void GetObjectTitle()
    {
        titleText.text = _currentInteractor.DisplayName;
    }

    private void GetObjectDescription()
    {
        descriptionText.text = _currentInteractor.Description;
    }
}