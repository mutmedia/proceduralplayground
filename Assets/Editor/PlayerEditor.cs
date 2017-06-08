using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerController player = (PlayerController)target;
        if (DrawDefaultInspector())
        {
        }

        if (GUILayout.Button("Grow"))
        {
            player.Grow();
        }
   }
}
