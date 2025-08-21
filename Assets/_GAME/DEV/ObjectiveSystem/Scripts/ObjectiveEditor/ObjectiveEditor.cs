using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

public class ObjectiveEditor : EditorWindow
{
    #region Variables For Creating New Objective

    // General fields for new objective
    private string _objectiveTitle;
    private string _objectiveDialogue;
    private Objective _objective;
    private ObjectiveType _objectiveType;
    private ObjectiveState _objectiveState;
    private bool _hasUnlockableObjects;

    // Relationships between objectives
    private ObjectiveDataSO _parentObjective;
    private ObjectiveDataSO _unlockingObjective;
    private List<ObjectiveDataSO> _childObjectives = new();
    private List<ObjectiveDataSO> _unlockableObjectives = new();

    // Fields for specific objective types
    private GameObject _targetObject;
    private Transform TargetPosition;
    private float Threshold = 3f;
    private string ItemId;
    private int RequiredCount;

    private const string ObjectiveSavePath = "Assets/_GAME/DEV/ObjectiveSystem/ScriptableObjects/Objectives/";

    #endregion

    #region Variables For Existing Objective

    private string[] existingObjectivePaths;
    private ObjectiveDataSO[] existingObjectives;
    private int selectedObjectiveIndex;
    private ObjectiveDataSO selectedObjective = null;

    #endregion

    [MenuItem("Morgan House Tools/Objective Editor")]
    public static void OpenObjectiveWindow()
    {
        GetWindow<ObjectiveEditor>("Objective Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create New Objective", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GeneralFields();
        ObjectiveFields();
        ObjectiveTypeFields();
        ObjectiveStateFields();
        HasUnlockableObjectsFields();

        EditorGUILayout.Space();
        SetCreateButton();

        if (GUILayout.Button("Save Changes") && selectedObjective != null)
        {
            SaveSelectedObjective(selectedObjective);
        }

        EditorGUILayout.Space();
        SetLoadExistingObjective();
    }

    #region UI Drawing Methods

    // Draws general fields used across all objectives
    private void GeneralFields()
    {
        _objective = (Objective)EditorGUILayout.EnumPopup("Objective", _objective);
        _objectiveType = (ObjectiveType)EditorGUILayout.EnumPopup("Objective Type", _objectiveType);
        _objectiveState = (ObjectiveState)EditorGUILayout.EnumPopup("Objective State", _objectiveState);

        GUI.color = string.IsNullOrEmpty(_objectiveTitle) ? Color.red : Color.white;
        _objectiveTitle = EditorGUILayout.TextField("Objective Name", _objectiveTitle);
        GUI.color = Color.white;

        _objectiveDialogue = EditorGUILayout.TextField("Objective Dialogue", _objectiveDialogue);
    }

    // Draws UI based on the selected ObjectiveType
    private void ObjectiveTypeFields()
    {
        EditorGUILayout.Space();

        switch (_objectiveType)
        {
            case ObjectiveType.CHILDOBJECTIVE:
                EditorGUILayout.Space();
                _parentObjective = (ObjectiveDataSO)EditorGUILayout.ObjectField("Parent Objective", _parentObjective, typeof(ObjectiveDataSO), true);
                break;

            case ObjectiveType.NORMALOBJECTIVE:
                EditorGUILayout.Space();
                _hasUnlockableObjects = EditorGUILayout.Toggle("Has Unlockable Objects", _hasUnlockableObjects);
                break;

            case ObjectiveType.PARENTOBJECTIVE:
                EditorGUILayout.Space();
                _hasUnlockableObjects = EditorGUILayout.Toggle("Has Unlockable Objects", _hasUnlockableObjects);
                EditorGUILayout.LabelField("Child Objectives", EditorStyles.boldLabel);

                for (int i = 0; i < _childObjectives.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _childObjectives[i] = (ObjectiveDataSO)EditorGUILayout.ObjectField($"Child {i + 1}", _childObjectives[i], typeof(ObjectiveDataSO), true);

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        _childObjectives.RemoveAt(i);
                        i--;
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Child Objective"))
                {
                    _childObjectives.Add(null);
                }
                break;
        }
    }

    // Draws fields based on the selected Objective type
    private void ObjectiveFields()
    {
        EditorGUILayout.Space();

        switch (_objective)
        {
            case Objective.SOLVEPUZZLE:
            case Objective.USE:
            case Objective.COLLECT:
                ItemId = EditorGUILayout.TextField("Item Id", ItemId);
                break;

            case Objective.REACH:
                _targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", _targetObject, typeof(GameObject), true);
                if (_targetObject != null)
                {
                    Transform targetTransform = _targetObject.transform;
                    Vector3 position = targetTransform.position;
                    EditorGUILayout.Vector3Field("Object Position", position);
                    TargetPosition = targetTransform;
                }
                Threshold = EditorGUILayout.FloatField("Threshold", Threshold);
                break;
        }
    }

    // Draws fields depending on selected objective state
    private void ObjectiveStateFields()
    {
        EditorGUILayout.Space();

        switch (_objectiveState)
        {
            case ObjectiveState.LOCKED:
                _unlockingObjective = (ObjectiveDataSO)EditorGUILayout.ObjectField("Unlocking Objective", _unlockingObjective, typeof(ObjectiveDataSO), true);
                break;
        }
    }

    // Draws list UI for unlockable objectives if applicable
    private void HasUnlockableObjectsFields()
    {
        EditorGUILayout.Space();

        if (_hasUnlockableObjects)
        {
            EditorGUILayout.LabelField("Unlockable Objectives", EditorStyles.boldLabel);

            for (int i = 0; i < _unlockableObjectives.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _unlockableObjectives[i] = (ObjectiveDataSO)EditorGUILayout.ObjectField($"Unlockable {i + 1}", _unlockableObjectives[i], typeof(ObjectiveDataSO), true);

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    _unlockableObjectives.RemoveAt(i);
                    i--;
                    continue;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Unlockable Objective"))
            {
                _unlockableObjectives.Add(null);
            }
        }
    }

    #endregion

    #region Objective Creation Methods

    // Draws "Create Objective" button
    private void SetCreateButton()
    {
        if (GUILayout.Button("Create New Objective"))
        {
            if (CheckForAllFilledFields())
            {
                CreateObjective();
            }
        }
    }

    // Creates and saves a new ScriptableObject based on input
    private void CreateObjective()
    {
        ObjectiveDataSO objectiveSO = null;

        switch (_objective)
        {
            case Objective.COLLECT:
                var collectSO = ScriptableObject.CreateInstance<CollectibleObjectiveSO>();
                collectSO.ItemId = ItemId;
                collectSO.RequiredCount = RequiredCount;
                objectiveSO = collectSO;
                break;

            case Objective.REACH:
                var reachSO = ScriptableObject.CreateInstance<ReachObjectiveSO>();
                reachSO.TargetPosition = TargetPosition;
                reachSO.Threshold = Threshold;
                objectiveSO = reachSO;
                break;

            case Objective.SOLVEPUZZLE:
                var puzzleSO = ScriptableObject.CreateInstance<SolvePuzzleObjectiveSO>();
                puzzleSO.ItemId = ItemId;
                objectiveSO = puzzleSO;
                break;

            case Objective.USE:
                var useSO = ScriptableObject.CreateInstance<UseObjectiveSO>();
                useSO.ItemId = ItemId;
                objectiveSO = useSO;
                break;
        }

        // Assign shared data
        objectiveSO.objectiveName = _objectiveTitle;
        objectiveSO.dialogDisplay = _objectiveDialogue;
        objectiveSO.objective = _objective;
        objectiveSO.objectiveType = _objectiveType;
        objectiveSO.objectiveState = _objectiveState;
        objectiveSO.ChildObjectives = _childObjectives;
        objectiveSO.UnlockOnComplete = _unlockableObjectives;
        objectiveSO.unlockingObjective = _unlockingObjective;

        Directory.CreateDirectory(ObjectiveSavePath);
        string saveTitle = _objectiveTitle.Replace(" ", "_");
        string assetPath = $"{ObjectiveSavePath}{saveTitle}_{_objectiveType}.asset";

        AssetDatabase.CreateAsset(objectiveSO, assetPath);
        AssetDatabase.SaveAssets();
    }

    // Validates all required fields
    private bool CheckForAllFilledFields()
    {
        if (string.IsNullOrWhiteSpace(_objectiveTitle))
        {
            EditorUtility.DisplayDialog("Missing Title", "Please enter the objective title.", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_objectiveDialogue))
        {
            EditorUtility.DisplayDialog("Missing Dialogue", "Please enter the dialogue.", "OK");
            return false;
        }

        if (_objectiveType == ObjectiveType.PARENTOBJECTIVE && _childObjectives.Count == 0)
        {
            EditorUtility.DisplayDialog("Missing Children", "Please add child objectives.", "OK");
            return false;
        }

        if (_objectiveType == ObjectiveType.CHILDOBJECTIVE && _parentObjective == null)
        {
            EditorUtility.DisplayDialog("Missing Parent", "Please assign a parent objective.", "OK");
            return false;
        }

        if (_objectiveState == ObjectiveState.LOCKED && _unlockingObjective == null)
        {
            EditorUtility.DisplayDialog("Missing Unlocker", "Please assign the unlocking objective.", "OK");
            return false;
        }

        if (_hasUnlockableObjects && _unlockableObjectives.Count == 0)
        {
            EditorUtility.DisplayDialog("Missing Unlockables", "Please assign unlockable objectives.", "OK");
            return false;
        }

        return true;
    }

    #endregion

    #region Objective Loading Methods

    // Draw UI to load saved objectives
    private void SetLoadExistingObjective()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Load Existing Objective", EditorStyles.boldLabel);

        if (GUILayout.Button("Load Objective List"))
        {
            LoadExistingObjective();
        }

        if (existingObjectives != null && existingObjectives.Length > 0)
        {
            selectedObjectiveIndex = EditorGUILayout.Popup("Selected Objective", selectedObjectiveIndex, existingObjectivePaths);

            if (GUILayout.Button("Load Selected Objective"))
            {
                LoadExistingObjectiveData(existingObjectives[selectedObjectiveIndex]);
            }
        }
    }

    // Load objective files from disk
    private void LoadExistingObjective()
    {
        string[] guids = AssetDatabase.FindAssets("t:ObjectiveDataSO", new[] { ObjectiveSavePath });
        existingObjectives = new ObjectiveDataSO[guids.Length];
        existingObjectivePaths = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            existingObjectives[i] = AssetDatabase.LoadAssetAtPath<ObjectiveDataSO>(path);
            existingObjectivePaths[i] = Path.GetFileNameWithoutExtension(path);
        }
    }

    // Populate editor with data from selected SO
    private void LoadExistingObjectiveData(ObjectiveDataSO objectiveSO)
    {
        selectedObjective = objectiveSO;
        _objectiveTitle = objectiveSO.objectiveName;
        _objectiveDialogue = objectiveSO.dialogDisplay;
        _objectiveType = objectiveSO.objectiveType;
        _objectiveState = objectiveSO.objectiveState;
        _childObjectives = objectiveSO.ChildObjectives;
        _unlockableObjectives = objectiveSO.UnlockOnComplete;
    }

    // Save current fields back to selected SO
    private void SaveSelectedObjective(ObjectiveDataSO objectiveSO)
    {
        objectiveSO.objectiveName = _objectiveTitle;
        objectiveSO.dialogDisplay = _objectiveDialogue;
        objectiveSO.objective = _objective;
        objectiveSO.objectiveType = _objectiveType;
        objectiveSO.objectiveState = _objectiveState;
        objectiveSO.ChildObjectives = _childObjectives;
        objectiveSO.UnlockOnComplete = _unlockableObjectives;

        EditorUtility.DisplayDialog("Update Successful", "Changes saved.", "OK");
        EditorUtility.SetDirty(objectiveSO);
        AssetDatabase.SaveAssets();
    }

    #endregion
}