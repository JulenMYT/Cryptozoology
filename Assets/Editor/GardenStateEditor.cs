using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GardenState))]
public class GardenStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GardenState garden = (GardenState)target;

        if (GUILayout.Button("Debug Garden State"))
        {
            garden.DebugGardenState();
        }
    }
}
