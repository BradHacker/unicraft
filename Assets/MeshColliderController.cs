using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshColliderController
{
  public static Dictionary<Chunk, MeshCollider> colliders = new Dictionary<Chunk, MeshCollider>();
  public static GameObject parent;

  public static void Initialize()
  {
    parent = new GameObject("Collider Controller");
  }

  public static void AddMeshCollider(Chunk chunk, Mesh mesh)
  {

  }
}
