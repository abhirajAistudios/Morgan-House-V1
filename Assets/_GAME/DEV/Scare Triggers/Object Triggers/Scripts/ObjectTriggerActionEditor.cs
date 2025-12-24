#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ObjectTriggerAction))]
public class ObjectTriggerActionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var actionTypeProp = serializedObject.FindProperty("actionType");
        EditorGUILayout.PropertyField(actionTypeProp);

        ObjectTriggerAction obj = (ObjectTriggerAction)target;

        switch ((TriggerActionType)actionTypeProp.enumValueIndex)
        {
            case TriggerActionType.ForceObject:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("forceDirection"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("forceAmount"));
                break;

            case TriggerActionType.FallingObject:
                EditorGUILayout.HelpBox("This object will fall when triggered.", MessageType.Info);
                break;

            case TriggerActionType.MoveObject:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveDirection"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveDistance"));
                break;

            case TriggerActionType.ShakeObject:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shakeDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shakeMagnitude"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dampingSpeed"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif