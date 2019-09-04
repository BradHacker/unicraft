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

  public int[,,] blockMap;
  int[,,][] faceMap;

  Mesh chunkMesh;
  MeshFilter meshFilter;
  MeshRenderer meshRenderer;
  Material masterMaterial;

  public Chunk(int chunkSize, int height, Vector2 location, Transform worldTransform, float[,,] noiseMap, Material material)
  {
    this.chunkSize = chunkSize;
    this.height = height;
    this.chunkOffset = location * chunkSize;
    this.noiseMap = noiseMap;
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
          // if (h < (int)(surfaceMap[x, y] * height))
          float sample = noiseMap[x, y, h];
          if (noiseMap[x, y, h] > .515f)
          {
            // float sample = PerlinNoise3D.Evaluate((chunkOffset.x + x) * noiseScale + noiseOffset, h * noiseScale + noiseOffset, (chunkOffset.y + y) * noiseScale + noiseOffset);
            if (sample > largest) largest = sample;
            if (sample < smallest) smallest = sample;
            // if (sample > .5f)
            // {
            int blockId = 2;
            if (noiseMap[x, y, h] > .52f) blockId = 1;

            if (h == 0) blockId = 0;
            // else if (h == (int)(surfaceMap[x, y] * height) - 1) blockId = 1;

            blockMap[x, y, h] = blockId;
          }
          else if (h == 0)
          {
            blockMap[x, y, h] = 0;
          }
          else
          {
            blockMap[x, y, h] = -1;
          }
        }
      }
    }

    // Debug.Log("Largest: " + largest);
    // Debug.Log("Smallest: " + smallest);
    RecalculateFaces();
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

  public void GenerateMesh()
  {
    // ArrayList blockTypes = new ArrayList();

    for (int x = 0; x < blockMap.GetLength(0); x++)
    {
      for (int y = 0; y < blockMap.GetLength(1); y++)
      {
        for (int h = 0; h < blockMap.GetLength(2); h++)
        {
          if (blockMap[x, y, h] != -1)
          {
            // if (!blockTypes.Contains(blockMap[x, y, h]))
            // {
            //   blockTypes.Add(blockMap[x, y, h]);
            // }

            if (h == 0)
            {
              BoxCollider collider = gameObject.AddComponent<BoxCollider>();
              collider.center = new Vector3(x + .5f, h - .5f, y + .5f);
            }
          }
        }
      }
    }

    // int textureLength = 0;
    // if (blockTypes.Count == 1) textureLength = 1;
    // else if (blockTypes.Count < 5) textureLength = 2;
    // else if (blockTypes.Count < 17) textureLength = 4;
    // else if (blockTypes.Count < 65) textureLength = 8;

    // // Combine the textures using block id and face index as a key
    // Dictionary<Vector2, Vector2> textureFaceAtlas = new Dictionary<Vector2, Vector2>();
    int originalSize = 16;
    int blockTextureLength = originalSize * 6;
    int textureSize = masterMaterial.mainTexture.height;

    // chunkTexture = new Texture2D(textureSize * 6, textureSize);
    // chunkTexture.filterMode = FilterMode.Point;

    // foreach (int id in blockTypes)
    // {
    //   // Debug.Log(id);
    //   int localX = (blockTextureLength * id) % chunkTexture.width;
    //   int localY = (blockTextureLength * id) / chunkTexture.width * originalSize;

    //   for (int i = 0; i < 6; i++)
    //   {
    //     if (!textureFaceAtlas.ContainsKey(new Vector2(id, i)))
    //     {
    //       Texture2D baseTexture = Resources.Load("Textures/" + id) as Texture2D;
    //       Texture2D sideTexture = Resources.Load("Textures/" + id + "_" + i) as Texture2D;

    //       // Debug.Log(baseTexture);
    //       // Debug.Log(sideTexture);

    //       chunkTexture.SetPixels(localX + (i * 16), localY, 16, 16, (sideTexture != null ? sideTexture : baseTexture).GetPixels());

    //       // Debug.Log("Id: " + id + " Face: " + i + " | " + (localX + (i * 16)) + "," + localY);

    //       textureFaceAtlas.Add(new Vector2(id, i), new Vector2(localX + (i * 16), localY));
    //     }
    //   }
    // }
    // chunkTexture.Apply();

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    int vertexIndex = 0;
    int triangleIndex = 0;

    for (int x = 0; x < faceMap.GetLength(0); x++)
    {
      for (int y = 0; y < faceMap.GetLength(1); y++)
      {
        for (int h = 0; h < faceMap.GetLength(2); h++)
        {
          if (faceMap[x, y, h] != null)
          {
            int id = blockMap[x, y, h];

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
                int localY = (blockTextureLength * id) / masterMaterial.mainTexture.width * originalSize;
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

    // Debug.Log(vertices[0].x + "," + vertices[0].y + "," + vertices[0].z);

    chunkMesh.vertices = vertices.ToArray();
    chunkMesh.triangles = triangles.ToArray();
    chunkMesh.uv = uvs.ToArray();
    chunkMesh.RecalculateNormals();

    meshFilter.sharedMesh = chunkMesh;
    meshRenderer.material = masterMaterial;

    // Resources.UnloadUnusedAssets();
  }

  public Mesh GetMesh()
  {
    return chunkMesh;
  }
}
