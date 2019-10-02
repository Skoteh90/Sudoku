using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cell))]
public class CellEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.BeginHorizontal();
            Cell myScript = (Cell)target;
            if(GUILayout.Button("Build Cell"))
            {
                myScript.BuildCell();
            }
            if(GUILayout.Button("Deconstruct Cell"))
            {
                myScript.DeconstructCell();
            }
        GUILayout.EndHorizontal();
    }
}
