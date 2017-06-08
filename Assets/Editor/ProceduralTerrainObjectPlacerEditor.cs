using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralTerrainObjectPlacer))]
public class ProceduralTerrainObjectPlacernEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProceduralTerrainObjectPlacer terrain = (ProceduralTerrainObjectPlacer)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Spawn Food"))
        {
            var x = UnityEngine.Random.value;
            var y = UnityEngine.Random.value;
            terrain.SpawnFood(x, y);
        }
    }
}
