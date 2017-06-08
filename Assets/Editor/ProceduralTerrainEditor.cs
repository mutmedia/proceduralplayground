using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralTerrain))]
public class ProceduralTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProceduralTerrain terrain = (ProceduralTerrain)target;
        if (DrawDefaultInspector())
        {
            if (terrain.AutoUpdate)
            {
                terrain.CreateTerrain();
            }
        }

        if (GUILayout.Button("Recreate terrain"))
        {
            terrain.CreateTerrain();
        }
    }
}
