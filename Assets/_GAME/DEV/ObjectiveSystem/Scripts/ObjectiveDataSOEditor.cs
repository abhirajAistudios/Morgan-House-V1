#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectiveDataSO), true)]
public class ObjectiveDataSOEditor : Editor
{
    private ObjectiveDataSO objective;

    private void OnEnable()
    {
        objective = (ObjectiveDataSO)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // General Fields
        EditorGUILayout.LabelField("General Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogDisplay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objective"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveState"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveStatus"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hasUnlockables"));
        EditorGUILayout.Space();

        // Draw Parent/Child/Unlocking Objectives
        DrawParentChildObjectiveFields();

        if (objective.hasUnlockables)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UnlockOnComplete"), true);
        }

        // Draw inherited fields based on actual type
        DrawSpecificObjectiveFields();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawParentChildObjectiveFields()
    {
        switch (objective.objectiveType)
        {
            case ObjectiveType.CHILDOBJECTIVE:
                EditorGUILayout.LabelField("Parent Objective", EditorStyles.boldLabel);
                objective.parentObjective = (ObjectiveDataSO)EditorGUILayout.ObjectField("Parent", objective.parentObjective, typeof(ObjectiveDataSO), false);
                break;

            case ObjectiveType.PARENTOBJECTIVE:
                EditorGUILayout.LabelField("Child Objectives", EditorStyles.boldLabel);
                SerializedProperty childList = serializedObject.FindProperty("ChildObjectives");

                for (int i = 0; i < childList.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(childList.GetArrayElementAtIndex(i), new GUIContent($"Child {i + 1}"));

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        childList.DeleteArrayElementAtIndex(i);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Child Objective"))
                {
                    childList.InsertArrayElementAtIndex(childList.arraySize);
                }
                break;
        }

        if (objective.objectiveState == ObjectiveState.LOCKED)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unlocking Objective", EditorStyles.boldLabel);
            objective.unlockingObjective = (ObjectiveDataSO)EditorGUILayout.ObjectField("Unlocks On Complete", objective.unlockingObjective, typeof(ObjectiveDataSO), false);
        }
    }

    private void DrawSpecificObjectiveFields()
    {
        // Cast and draw custom fields for derived types
        switch (objective)
        {
            case CollectibleObjectiveSO collect:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Collectible Objective", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemId"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequiredCount"));
                break;

            case UseObjectiveSO use:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Use Objective", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemId"));
                break;

            case ReachObjectiveSO reach:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Reach Objective", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetPosition"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Threshold"));
                break;

            case SolvePuzzleObjectiveSO solve:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Solve Puzzle Objective", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemId"));
                break;
        }
    }
}
#endif