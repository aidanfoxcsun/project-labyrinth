using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeGraphGenerator))]
public class NodeGraphGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NodeGraphGenerator generator = (NodeGraphGenerator)target;

        if(GUILayout.Button("Generate Graph"))
        {
            generator.GenerateGraph();
        }

        if(GUILayout.Button("Clear Graph"))
        {
            generator.ClearGraph();
        }

    }

    private void OnSceneGUI()
    {
        NodeGraphGenerator generator = (NodeGraphGenerator)target;

        EditorGUI.BeginChangeCheck();

        Vector3 min = Vector2.Min(generator.boundA, generator.boundB);
        Vector3 max = Vector2.Max(generator.boundA, generator.boundB);

        min = Handles.PositionHandle(min, Quaternion.identity);
        max = Handles.PositionHandle(max, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(generator, "Move Grid Bounds");
            generator.boundA = min;
            generator.boundB = max;
        }
    }
}
