using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Block
{
  public GameObject gameObject;

  const int resolution = 2;
  public int id;
  public Vector3 position;
  public BlockFace[] faces;
  public MeshFilter[] meshFilters;
  public MeshRenderer[] meshRenderers;
  public GameObject[] meshObjs;
  public Texture2D[] textures = new Texture2D[6];

  Vector2[] baseUv = new Vector2[4] {
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
        new Vector2(0f, 0f),
        new Vector2(1f, 0f)
      };

  Vector2[] rotatedUvFor4 = new Vector2[4] {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
      };
  Vector2[] rotatedUvFor5 = new Vector2[4] {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
      };

  public Block(int id, Vector3 position, Transform chunkTransform)
  {
    this.id = id;
    this.position = position;

    gameObject = new GameObject("Block (" + position.x + "," + position.z + "," + position.y + ")");
    gameObject.transform.parent = chunkTransform;
    gameObject.AddComponent<BoxCollider>().center = position + new Vector3(.5f, -.5f, .5f);

    RotateUVs(ref rotatedUvFor4, Mathf.PI / 2);
    RotateUVs(ref rotatedUvFor5, -Mathf.PI / 2);
  }

  public void Initialize(Block[] neighbors)
  {
    meshFilters = new MeshFilter[6];
    meshRenderers = new MeshRenderer[6];
    meshObjs = new GameObject[6];
    faces = new BlockFace[6];

    Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    for (int i = 0; i < 6; i++)
    {
      if (neighbors[i] == null)
      {
        Texture2D baseTexture = Resources.Load("Textures/" + id) as Texture2D;
        Texture2D sideTexture = Resources.Load("Textures/" + id + "_" + i) as Texture2D;

        if (meshFilters[i] == null)
        {
          GameObject meshObj = new GameObject("mesh " + i);
          meshObj.transform.parent = gameObject.transform;
          meshObjs[i] = meshObj;

          Material blockMaterial = new Material(Shader.Find("Standard"));
          blockMaterial.SetTexture("_MainTex", sideTexture != null ? sideTexture : baseTexture);
          textures[i] = sideTexture != null ? sideTexture : baseTexture;

          meshRenderers[i] = meshObj.AddComponent<MeshRenderer>();
          meshRenderers[i].sharedMaterial = blockMaterial;

          meshFilters[i] = meshObj.AddComponent<MeshFilter>();
          meshFilters[i].sharedMesh = new Mesh();
        }

        Vector2[] uvs = baseUv;
        if (i == 4) uvs = rotatedUvFor4;
        if (i == 5) uvs = rotatedUvFor5;

        faces[i] = new BlockFace(meshFilters[i].sharedMesh, resolution, directions[i], position, uvs);
      }
    }
  }

  void GenerateMesh()
  {
    foreach (BlockFace face in faces)
    {
      if (face != null)
      {
        face.ConstructMesh();
      }
    }
  }

  public void RecalculateFaces(Block[] neighbors)
  {
    Initialize(neighbors);
    GenerateMesh();
  }

  void RotateUVs(ref Vector2[] uvs, float rotationRadians)
  {
    float rotMatrix00 = Mathf.Cos(rotationRadians);
    float rotMatrix01 = -Mathf.Sin(rotationRadians);
    float rotMatrix10 = Mathf.Sin(rotationRadians);
    float rotMatrix11 = Mathf.Cos(rotationRadians);

    Vector2 halfVector = new Vector2(0.5f, 0.5f);

    for (int j = 0; j < uvs.Length; j++)
    {
      // Switch coordinates to be relative to center of the plane
      uvs[j] = uvs[j] - halfVector;
      // Apply the rotation matrix
      float u = rotMatrix00 * uvs[j].x + rotMatrix01 * uvs[j].y;
      float v = rotMatrix10 * uvs[j].x + rotMatrix11 * uvs[j].y;
      uvs[j].x = u;
      uvs[j].y = v;
      // Switch back coordinates to be relative to edge
      uvs[j] = uvs[j] + halfVector;
    }
  }
}
