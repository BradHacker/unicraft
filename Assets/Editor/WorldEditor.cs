using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    World world = (World)target;

    if (GUILayout.Button("Generate World"))
    {
      world.Generate();
    }

    if (GUILayout.Button("Combine Meshes"))
    {
      world.CombineMeshes();
    }
  }
}