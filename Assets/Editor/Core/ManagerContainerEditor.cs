#if UNITY_EDITOR
using Core.Managers;
using UnityEditor;

[CustomEditor(typeof(ManagerContainer))]
public class ManagerContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ManagerContainer container = (ManagerContainer)target;
        if (container._managers != null && container._managers.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Loaded Managers:", EditorStyles.boldLabel);
            foreach (var manager in container._managers)
            {
                EditorGUILayout.LabelField($"• {manager.GetType().Name}");
            }
        }
    }
}
#endif