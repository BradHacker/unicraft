using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshColliderController
{
  public static Dictionary<Chunk, MeshCollider> colliders = new Dictionary<Chunk, MeshCollider>();
  public static Queue<MeshCollider> colliderPool = new Queue<MeshCollider>();
  public static GameObject parent;
  public static PhysicMaterial physicMaterial;

  public static void Initialize()
  {
    if (GameObject.Find("Collider Controller") != null)
    {
      parent = GameObject.Find("Collider Controller");
    }
    else
    {
      parent = new GameObject("Collider Controller");
    }

    physicMaterial = new PhysicMaterial();
    physicMaterial.bounciness = 0;
    physicMaterial.dynamicFriction = 0;
    physicMaterial.staticFriction = .6f;
    physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
  }

  public static void AddMeshCollider(Chunk chunk, Mesh mesh)
  {
    MeshCollider collider;
    if (colliders.TryGetValue(chunk, out collider))
    {
      collider.sharedMesh = mesh;
    }
    else
    {
      if (colliderPool.Count > 0)
      {
        collider = colliderPool.Dequeue();
      }
      else
      {
        GameObject go = new GameObject();
        go.transform.SetParent(parent.transform);

        collider = go.AddComponent<MeshCollider>();
        collider.cookingOptions = MeshColliderCookingOptions.None;
      }

      collider.sharedMesh = mesh;
      collider.material = physicMaterial;
      colliders.Add(chunk, collider);
    }
  }

  public static void RemoveMeshCollider(Chunk chunk)
  {
    MeshCollider collider;
    if (colliders.TryGetValue(chunk, out collider))
    {
      colliders.Remove(chunk);
      colliderPool.Enqueue(collider);
    }
  }

  public static void ClearColliders()
  {
    for (int i = parent.transform.childCount - 1; i >= 0; i--)
    {
      GameObject.DestroyImmediate(parent.transform.GetChild(i).gameObject);
    }
    colliders = new Dictionary<Chunk, MeshCollider>();
    colliderPool = new Queue<MeshCollider>();
  }
}
