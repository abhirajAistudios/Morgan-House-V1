using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ObjectiveUIManager : MonoBehaviour
{
    [SerializeField] private GameObject TextViewContent;
    [SerializeField] private GameObject ParentTextPrefab;
    [SerializeField] private GameObject ChildTextPrefab;

    private ObjectiveManager objectiveManager;
    private List<TMP_Text> spawnedTexts = new();

    // NEW: Track Objectives that had their UI destroyed
    public HashSet<ObjectiveDataSO> uiRemovedObjectives = new();
    
    private void Start()
    {
        objectiveManager = FindAnyObjectByType<ObjectiveManager>();
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
            // SKIP objectives whose UI has already been destroyed
            if (uiRemovedObjectives.Contains(objective))
                continue;

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

        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED)
        {
            if ((isParent || parent == null) && gameObject.activeInHierarchy && textObj.activeInHierarchy)
                StartCoroutine(DestroyAfterDelay(objective, textObj, 1.0f));
            else if(gameObject.activeInHierarchy)
                StartCoroutine(WaitForParentThenDestroy(objective, parent, textObj));
        }

        return text;
    }

    private IEnumerator DestroyAfterDelay(ObjectiveDataSO objective, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
            Destroy(obj);
            uiRemovedObjectives.Add(objective);  // Mark as UI removed
        
        RefreshUI();
    }

    private IEnumerator WaitForParentThenDestroy(ObjectiveDataSO child, ObjectiveDataSO parent, GameObject obj)
    {
        while (parent.objectiveStatus != ObjectiveStatus.COMPLETED)
            yield return null;

        yield return new WaitForSeconds(3f);

        if (obj != null)
        {
            Destroy(obj);
            uiRemovedObjectives.Add(child);  // Mark child as UI removed
        }
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