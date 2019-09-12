using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Chunk
{
  GameObject gameObject;

  Vector2 chunkOffset;
  int chunkSize;
  int height;
  float[,,] noiseMap;
  float[,] surfaceMap;

  public int[,,] blockMap;
  int[,,][] faceMap;

  Mesh chunkMesh;
  MeshFilter meshFilter;
  MeshRenderer meshRenderer;
  Material masterMaterial;

  List<Vector3> vertices = new List<Vector3>();
  List<Vector2> uvs = new List<Vector2>();
  List<int> triangles = new List<int>();

  public Chunk(int chunkSize, int height, Vector2 location, Transform worldTransform, float[,,] noiseMap, float[,] surfaceMap, Material material)
  {
    this.chunkSize = chunkSize;
    this.height = height;
    this.chunkOffset = location * chunkSize;
    this.noiseMap = noiseMap;
    this.surfaceMap = surfaceMap;
    this.masterMaterial = material;

    gameObject = new GameObject("Chunk (" + location.x + "," + location.y + ")");
    gameObject.transform.parent = worldTransform;

    blockMap = new int[chunkSize, chunkSize, height];
    faceMap = new int[chunkSize, chunkSize, height][];

    chunkMesh = new Mesh();
    chunkMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    meshFilter = gameObject.AddComponent<MeshFilter>();
    meshRenderer = gameObject.AddComponent<MeshRenderer>();

    Initialize();
    RecalculateFaces();
  }

  void Initialize()
  {
    float largest = float.MinValue;
    float smallest = float.MaxValue;

    for (int h = 0; h < height; h++)
    {
      for (int y = 0; y < chunkSize; y++)
      {
        for (int x = 0; x < chunkSize; x++)
        {
          blockMap[x, y, h] = -1;

          if (h <= (int)surfaceMap[x, y] && Block.GetBlockFromNoise(1, noiseMap[x, y, h]) > -1)
          {
            float sample = noiseMap[x, y, h];

            // float sample = PerlinNoise3D.Evaluate((chunkOffset.x + x) * noiseScale + noiseOffset, h * noiseScale + noiseOffset, (chunkOffset.y + y) * noiseScale + noiseOffset);
            if (sample > largest) largest = sample;
            if (sample < smallest) smallest = sample;
            // if (sample > .5f)
            // {
            int blockId = Block.GetBlockFromNoise(1, noiseMap[x, y, h]);
            // else if (h == (int)(surfaceMap[x, y] * height) - 1) blockId = 1;

            blockMap[x, y, h] = blockId;

            if (h == (int)surfaceMap[x, y]) blockMap[x, y, h] = 1;
          }

          if (h == 0) blockMap[x, y, h] = 0;
        }
      }
    }

    // Debug.Log("Largest: " + largest);
    // Debug.Log("Smallest: " + smallest);
  }

  void RecalculateFaces()
  {
    for (int x = 0; x < blockMap.GetLength(0); x++)
    {
      for (int y = 0; y < blockMap.GetLength(1); y++)
      {
        for (int h = 0; h < blockMap.GetLength(2); h++)
        {
          if (blockMap[x, y, h] != -1)
          {
            faceMap[x, y, h] = new int[6] {
                h < height - 1 ? blockMap[x, y, h + 1] : -1,
                h > 0 ? blockMap[x, y, h - 1] : -1,
                x > 0 ? blockMap[x - 1, y, h] : -1,
                x < chunkSize - 1 ? blockMap[x + 1, y, h] : -1,
                y < chunkSize - 1 ? blockMap[x, y + 1, h] : -1,
                y > 0 ? blockMap[x, y - 1, h] : -1
            };
          }
        }
      }
    }
  }

  void UpdateFaceAtBlock(Vector3 blockPosition)
  {
    for (int x = (int)blockPosition.x - 1; x <= (int)blockPosition.x + 1; x++)
    {
      for (int y = (int)blockPosition.z - 1; y <= (int)blockPosition.z + 1; y++)
      {
        for (int h = (int)blockPosition.y; h <= (int)blockPosition.y + 1; h++)
        {
          if (x >= 0 && x < chunkSize && y >= 0 && y < chunkSize && h >= 0 && x < height)
          {
            if (blockMap[x, y, h] != -1)
            {
              faceMap[x, y, h] = new int[6] {
                h < height - 1 ? blockMap[x, y, h + 1] : -1,
                h > 0 ? blockMap[x, y, h - 1] : -1,
                x > 0 ? blockMap[x - 1, y, h] : -1,
                x < chunkSize - 1 ? blockMap[x + 1, y, h] : -1,
                y < chunkSize - 1 ? blockMap[x, y + 1, h] : -1,
                y > 0 ? blockMap[x, y - 1, h] : -1
              };
            }
            else
            {
              faceMap[x, y, h] = null;
            }
          }
        }
      }
    }
  }

  public void GenerateMesh()
  {
    int originalSize = 16;
    int blockTextureLength = originalSize * 6;
    int textureSize = masterMaterial.mainTexture.height;

    int vertexIndex = 0;
    int triangleIndex = 0;

    for (int x = 0; x < faceMap.GetLength(0); x++)
    {
      for (int y = 0; y < faceMap.GetLength(1); y++)
      {
        for (int h = 0; h < faceMap.GetLength(2); h++)
        {
          int id = blockMap[x, y, h];

          if (id != -1 && faceMap[x, y, h] != null)
          {
            for (int f = 0; f < faceMap[x, y, h].Length; f++)
            {
              if (faceMap[x, y, h][f] == -1)
              {
                Vector3 position = new Vector3(chunkOffset.x + x, h, chunkOffset.y + y);
                Vector3 offset = new Vector3(.5f, -.5f, .5f) + position;

                Vector3 localUp = Block.directions[f];
                Vector3 xAxis = new Vector3(localUp.y, localUp.z, localUp.x);
                Vector3 zAxis = Vector3.Cross(localUp, xAxis);

                vertices.Add(localUp / 2 + (0f - .5f) * xAxis + (0f - .5f) * zAxis + offset);
                vertices.Add(localUp / 2 + (1f - .5f) * xAxis + (0f - .5f) * zAxis + offset);
                vertices.Add(localUp / 2 + (1f - .5f) * xAxis + (1f - .5f) * zAxis + offset);
                vertices.Add(localUp / 2 + (0f - .5f) * xAxis + (1f - .5f) * zAxis + offset);

                int localX = (blockTextureLength * id) % masterMaterial.mainTexture.width;
                int localY = ((blockTextureLength * id) / masterMaterial.mainTexture.width) * originalSize;
                Vector2 uvOffset = new Vector2(localX + (f * 16), localY);
                Vector2 adjustedUv0 = ((Block.faceUvMaps[f][0] * originalSize) + uvOffset) / new Vector2(textureSize * 6, textureSize);
                Vector2 adjustedUv1 = ((Block.faceUvMaps[f][1] * originalSize) + uvOffset) / new Vector2(textureSize * 6, textureSize);
                Vector2 adjustedUv2 = ((Block.faceUvMaps[f][2] * originalSize) + uvOffset) / new Vector2(textureSize * 6, textureSize);
                Vector2 adjustedUv3 = ((Block.faceUvMaps[f][3] * originalSize) + uvOffset) / new Vector2(textureSize * 6, textureSize);
                uvs.Add(adjustedUv0);
                uvs.Add(adjustedUv1);
                uvs.Add(adjustedUv2);
                uvs.Add(adjustedUv3);

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);

                vertexIndex += 4;
                triangleIndex += 6;
              }
            }
          }
        }
      }
    }

    // Debug.Log("Vertices: " + vertices.ToArray().Length + " | Triangles: " + triangles.ToArray().Length);

    // chunkMesh.Clear();
    chunkMesh.vertices = vertices.ToArray();
    chunkMesh.triangles = triangles.ToArray();
    chunkMesh.uv = uvs.ToArray();
    chunkMesh.RecalculateNormals();

    meshFilter.sharedMesh = chunkMesh;
    meshRenderer.material = masterMaterial;

    MeshColliderController.AddMeshCollider(this, chunkMesh);

    // Resources.UnloadUnusedAssets();
  }

  public Mesh GetMesh()
  {
    return chunkMesh;
  }

  void ClearMesh()
  {
    chunkMesh.Clear();
    vertices.Clear();
    uvs.Clear();
    triangles.Clear();
    MeshColliderController.RemoveMeshCollider(this);
  }

  public void BreakBlockAtPoint(Vector3 worldPoint)
  {
    ClearMesh();
    Vector3 chunkPoint = new Vector3((int)worldPoint.x % chunkSize, worldPoint.y, (int)worldPoint.z % chunkSize);
    // Debug.Log("Chunk Point: " + chunkPoint);
    // Debug.Log("Block To Break: " + blockMap[(int)chunkPoint.x, (int)chunkPoint.z, (int)chunkPoint.y]);
    blockMap[(int)chunkPoint.x, (int)chunkPoint.z, (int)chunkPoint.y] = -1;
    // UpdateFaceAtBlock(chunkPoint);
    RecalculateFaces();
    GenerateMesh();
  }
}
