using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ObjectiveUIManager : MonoBehaviour
{
    public GameObject TextViewContent;
    public GameObject ParentTextPrefab;
    public GameObject ChildTextPrefab;

    private ObjectiveManager objectiveManager;
    private List<TMP_Text> spawnedTexts = new();

    private void Awake()
    {
        objectiveManager = FindAnyObjectByType<ObjectiveManager>();
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (var text in spawnedTexts)
        {
            if (text != null)
                Destroy(text.gameObject);
        }
        spawnedTexts.Clear();

        var allObjectives = objectiveManager.activeObjectives
            .Union(objectiveManager.completedObjectives)
            .Distinct()
            .ToList();

        foreach (var objective in allObjectives)
        {
            if (IsChildObjective(objective)) continue;

            var parentText = CreateTextElement(objective, true);
            spawnedTexts.Add(parentText);

            if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
            {
                foreach (var child in objective.ChildObjectives)
                {
                    var childText = CreateTextElement(child, false, objective);
                    spawnedTexts.Add(childText);
                }
            }
        }
    }

    private TMP_Text CreateTextElement(ObjectiveDataSO objective, bool isParent, ObjectiveDataSO parent = null)
    {
        GameObject prefab = isParent ? ParentTextPrefab : ChildTextPrefab;
        GameObject textObj = Instantiate(prefab, TextViewContent.transform);
        TMP_Text text = textObj.GetComponent<TMP_Text>();

        string prefix = objective.objectiveStatus == ObjectiveStatus.COMPLETED
            ? "<color=green>[✓]</color> "
            : "<color=white>[ ]</color> ";

        text.text = prefix + objective.dialogDisplay;

        // Schedule destroy only if completed
        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED)
        {
            if (isParent || parent == null)
            {
                // Top-level or has no parent → destroy after delay
                StartCoroutine(DestroyAfterDelay(textObj, 3f));
            }
            else
            {
                // Is a child — wait until parent is also completed
                StartCoroutine(WaitForParentThenDestroy(objective, parent, textObj));
            }
        }

        return text;
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
    }

    private IEnumerator WaitForParentThenDestroy(ObjectiveDataSO child, ObjectiveDataSO parent, GameObject obj)
    {
        // Wait until parent is completed
        while (parent.objectiveStatus != ObjectiveStatus.COMPLETED)
            yield return null;

        // Wait 3 seconds after parent is completed
        yield return new WaitForSeconds(3f);

        if (obj != null)
            Destroy(obj);
    }

    private bool IsChildObjective(ObjectiveDataSO objective)
    {
        foreach (var parent in objectiveManager.activeObjectives
                     .Union(objectiveManager.completedObjectives))
        {
            if (parent.ChildObjectives != null && parent.ChildObjectives.Contains(objective))
                return true;
        }
        return false;
    }

    public void OnObjectiveUpdated()
    {
        RefreshUI();
    }
}
