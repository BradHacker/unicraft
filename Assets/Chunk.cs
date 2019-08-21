using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Chunk
{
  float noiseScale = .05f;
  float noiseOffset;

  GameObject gameObject;
  public Block[,,] blocks;
  Vector2 chunkOffset;
  int chunkSize;
  int height;
  float[,] surfaceMap;

  MeshFilter meshFilter;
  MeshRenderer meshRenderer;
  Material material;

  Material combinedMaterial;

  public Chunk(int chunkSize, int height, Vector2 location, Transform worldTransform, float[,] surfaceMap, float noiseOffset)
  {
    this.chunkSize = chunkSize;
    this.height = height;
    this.chunkOffset = location * chunkSize;
    this.surfaceMap = surfaceMap;
    this.noiseOffset = noiseOffset;

    gameObject = new GameObject("Chunk (" + location.x + "," + location.y + ")");
    gameObject.transform.parent = worldTransform;

    Initialize();
  }

  void Initialize()
  {
    float largest = float.MinValue;
    float smallest = float.MaxValue;
    if (blocks != null && blocks.Length > 0)
    {
      blocks = null;
    }
    blocks = new Block[chunkSize, chunkSize, height];

    for (int h = 0; h < height; h++)
    {
      for (int y = 0; y < chunkSize; y++)
      {
        for (int x = 0; x < chunkSize; x++)
        {
          if (h < (int)(surfaceMap[x, y] * height))
          {
            float sample = PerlinNoise3D.Evaluate((chunkOffset.x + x) * noiseScale + noiseOffset, h * noiseScale + noiseOffset, (chunkOffset.y + y) * noiseScale + noiseOffset);
            if (sample > largest) largest = sample;
            if (sample < smallest) smallest = sample;
            if (sample > .5f)
            {
              int blockId = 2;

              if (h == 0) blockId = 0;
              else if (h == (int)(surfaceMap[x, y] * height) - 1) blockId = 1;

              Vector3 blockLocation = new Vector3(x, h, y);
              Vector3 blockPosition = new Vector3(chunkOffset.x + x, h, chunkOffset.y + y);
              blocks[x, y, h] = new Block(blockId, blockPosition, gameObject.transform);
            }
            else if (h == 0)
            {
              Vector3 blockLocation = new Vector3(x, h, y);
              Vector3 blockPosition = new Vector3(chunkOffset.x + x, h, chunkOffset.y + y);
              blocks[x, y, h] = new Block(0, blockPosition, gameObject.transform);
            }
          }
        }
      }
    }
    // Debug.Log("Optimizing faces...");
    RecalculateFaces();
    // Debug.Log("Smallest: " + smallest + " Largest: " + largest);
  }

  void RecalculateFaces()
  {
    for (int h = 0; h < blocks.GetLength(2); h++)
    {
      for (int y = 0; y < blocks.GetLength(1); y++)
      {
        for (int x = 0; x < blocks.GetLength(0); x++)
        {
          if (blocks[x, y, h] != null)
          {
            blocks[x, y, h].RecalculateFaces(new Block[6] {
                h < height - 1 ? blocks[x, y, h + 1] : null,
                h > 0 ? blocks[x, y, h - 1] : null,
                x > 0 ? blocks[x - 1, y, h] : null,
                x < chunkSize - 1 ? blocks[x + 1, y, h] : null,
                y < chunkSize - 1 ? blocks[x, y + 1, h] : null,
                y > 0 ? blocks[x, y - 1, h] : null
            });
          }
        }
      }
    }
  }

  public void CombineMeshes()
  {
    // Determine size of combined texture (square)
    ArrayList blockTypes = new ArrayList();

    for (int x = 0; x < blocks.GetLength(0); x++)
    {
      for (int y = 0; y < blocks.GetLength(1); y++)
      {
        for (int h = 0; h < blocks.GetLength(2); h++)
        {
          if (blocks[x, y, h] != null)
          {
            if (!blockTypes.Contains(blocks[x, y, h].id))
            {
              blockTypes.Add(blocks[x, y, h].id);
            }
          }
        }
      }
    }

    // Debug.Log("Block Types Found: " + blockTypes.Count);

    int textureLength = 0;
    if (blockTypes.Count == 1) textureLength = 1;
    else if (blockTypes.Count < 5) textureLength = 2;
    else if (blockTypes.Count < 17) textureLength = 4;
    else if (blockTypes.Count < 65) textureLength = 8;

    // Combine the textures using block id as a key
    Dictionary<int, Vector2> textureAtlas = new Dictionary<int, Vector2>();
    Dictionary<Vector2, Vector2> textureFaceAtlas = new Dictionary<Vector2, Vector2>();

    int originalSize = 16;
    int blockTextureLength = originalSize * 6;
    int textureSize = textureLength * originalSize;
    int blockCount = 0;

    // Debug.Log("Texture Length: " + textureLength);
    // Debug.Log("Texture Size : " + textureSize);

    Texture2D combinedTexture = new Texture2D(textureSize * 6, textureSize);
    combinedTexture.filterMode = FilterMode.Point;

    for (int x = 0; x < blocks.GetLength(0); x++)
    {
      for (int y = 0; y < blocks.GetLength(1); y++)
      {
        for (int h = 0; h < blocks.GetLength(2); h++)
        {
          if (blocks[x, y, h] != null)
          {
            blockCount++;
            int id = blocks[x, y, h].id;

            if (!textureAtlas.ContainsKey(id))
            {
              int localX = (blockTextureLength * id) % combinedTexture.width;
              int localY = (blockTextureLength * id) / combinedTexture.width * originalSize;

              // Debug.Log("Block Debug " + (blockTextureLength * id) + " and " + combinedTexture.width);
              // Debug.Log("Block " + id + " at " + localX + "," + localY);

              for (int i = 0; i < blocks[x, y, h].textures.Length; i++)
              {
                Texture2D baseTexture = Resources.Load("Textures/" + id) as Texture2D;
                Texture2D sideTexture = Resources.Load("Textures/" + id + "_" + i) as Texture2D;
                combinedTexture.SetPixels(localX + (i * 16), localY, 16, 16, (sideTexture != null ? sideTexture : baseTexture).GetPixels());

                textureFaceAtlas.Add(new Vector2(id, i), new Vector2(localX + (i * 16), localY));
              }

              textureAtlas.Add(id, new Vector2(localX, localY));
            }
          }
        }
      }
    }
    combinedTexture.Apply();
    // File.WriteAllBytes("Assets/output.png", combinedTexture.EncodeToPNG());

    combinedMaterial = new Material(Shader.Find("Standard"));
    combinedMaterial.mainTexture = combinedTexture;

    int faceCount = 0;

    // Revise uv maps of all blocks
    for (int x = 0; x < blocks.GetLength(0); x++)
    {
      for (int y = 0; y < blocks.GetLength(1); y++)
      {
        for (int h = 0; h < blocks.GetLength(2); h++)
        {
          if (blocks[x, y, h] != null)
          {
            Block block = blocks[x, y, h];
            int id = block.id;

            for (int i = 0; i < 6; i++)
            {
              if (block.meshFilters[i] != null)
              {
                faceCount++;
                Mesh mesh = block.meshFilters[i].sharedMesh;
                Vector2[] uv = new Vector2[mesh.uv.Length];
                Vector2 offset;
                if (textureAtlas.ContainsKey(id))
                {
                  offset = (Vector2)textureFaceAtlas[new Vector2(id, i)];
                  // Debug.Log("Block " + id + " offset is " + offset.x + "," + offset.y);
                  for (int u = 0; u < uv.Length; u++)
                  {
                    uv[u] = mesh.uv[u] * originalSize;
                    // Local Zero offset + face offset
                    uv[u].x += ((float)offset.x);
                    uv[u].y += ((float)offset.y);
                    uv[u].x /= (float)(textureSize * 6);
                    uv[u].y /= (float)textureSize;

                  }
                }
                mesh.uv = uv;
                block.meshRenderers[i].sharedMaterial = combinedMaterial;
              }
            }
          }
        }
      }
    }

    // Combine all meshes
    CombineInstance[] combine = new CombineInstance[faceCount];
    int faceIndex = 0;
    for (int x = 0; x < blocks.GetLength(0); x++)
    {
      for (int y = 0; y < blocks.GetLength(1); y++)
      {
        for (int h = 0; h < blocks.GetLength(2); h++)
        {
          if (blocks[x, y, h] != null)
          {
            Block block = blocks[x, y, h];

            for (int i = 0; i < block.meshFilters.Length; i++)
            {
              if (block.meshFilters[i] != null)
              {
                combine[faceIndex].mesh = block.meshFilters[i].sharedMesh;
                combine[faceIndex].transform = block.meshObjs[i].transform.localToWorldMatrix;
                faceIndex++;
              }
            }
          }
        }
      }
    }

    MeshFilter filter = gameObject.AddComponent<MeshFilter>();
    MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
    filter.sharedMesh = new Mesh();
    filter.sharedMesh.CombineMeshes(combine);
    renderer.material = combinedMaterial;

    for (int x = 0; x < blocks.GetLength(0); x++)
    {
      for (int y = 0; y < blocks.GetLength(1); y++)
      {
        for (int h = 0; h < blocks.GetLength(2); h++)
        {
          if (blocks[x, y, h] != null)
          {
            for (int i = 0; i < blocks[x, y, h].meshFilters.Length; i++)
            {
              if (blocks[x, y, h].meshFilters[i] != null)
              {
                blocks[x, y, h].meshFilters[i].mesh = null;
                blocks[x, y, h].meshRenderers[i].material = null;
                blocks[x, y, h].gameObject.SetActive(false);
              }
            }
          }
        }
      }
    }

    Resources.UnloadUnusedAssets();
  }
}
