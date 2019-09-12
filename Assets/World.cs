using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
  public int seed;
  [Range(8, 16)]
  public int chunkSize = 8;
  [Range(1, 100)]
  public int chunkHeight = 8;
  public int worldSize = 1;

  [Range(.1f, .5f)]
  public float noiseScale = .05f;
  [Range(.001f, 1f)]
  public float surfaceNoiseScale = .01f;
  public float surfaceNoiseStrength = 1;
  public int minSurfaceHeight = 5;
  public NoiseSettings noiseSettings;
  NoiseFilter noiseFilter;

  public Player player;

  public Chunk[,] chunks;
  // Dictionary<Chunk, MeshCollider> colliders = new Dictionary<Chunk, MeshCollider>();

  public Texture2D masterTexture;
  public Material masterMaterial;

  void Start()
  {
    player = GameObject.Find("Player").GetComponent<Player>();
    // noise = new SimplexNoiseGenerator(seed);
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    // Physics.gravity = new Vector3(0, -1.5f, 0);

    Generate();
  }

  public World()
  {
    noiseSettings = new NoiseSettings();
    noiseFilter = new NoiseFilter(seed, noiseSettings);
  }

  public void Generate()
  {
    // if (player == null) player = GameObject.Find("Player").GetComponent<Player>();
    // if (noise == null) noise = new SimplexNoiseGenerator(seed);
    masterMaterial = new Material(Shader.Find("Standard"));
    masterMaterial.mainTexture = masterTexture;
    masterMaterial.SetFloat("_Smoothness", 0);
    // masterMaterial.SetFloat("_Mode", 3);

    MeshColliderController.Initialize();

    if (chunks != null && chunks.Length > 0)
    {
      for (int y = 0; y < chunks.GetLength(1); y++)
      {
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
          // Debug.Log(chunks[x, y].GetMesh());
          // MeshCollider collider = gameObject.AddComponent<MeshCollider>();
          // collider.sharedMesh = chunks[x, y].GetMesh();
          // colliders[chunks[x, y]] = collider;
          MeshColliderController.RemoveMeshCollider(chunks[x, y]);
        }
      }
    }

    Debug.Log("Starting Map Generation...");

    if (transform.childCount > 0) ClearChunks();

    // Debug.Log("Generating new chunks...");
    chunks = new Chunk[worldSize, worldSize];

    for (int y = 0; y < worldSize; y++)
    {
      for (int x = 0; x < worldSize; x++)
      {
        // Debug.Log("Chunk " + (y * worldSize + x) + "/" + (worldSize * worldSize) + " loaded...");
        chunks[x, y] = new Chunk(chunkSize, chunkHeight, new Vector2(x, y), transform, GetDeepNoise(x, y), GetSurfaceNoise(x, y), masterMaterial);
      }
    }

    GenerateMeshes();

    // for (int y = 0; y < worldSize; y++)
    // {
    //   for (int x = 0; x < worldSize; x++)
    //   {
    //     if (chunks[x, y].GetMesh() != null)
    //     {
    //       // Debug.Log(chunks[x, y].GetMesh());
    //       // MeshCollider collider = gameObject.AddComponent<MeshCollider>();
    //       // collider.sharedMesh = chunks[x, y].GetMesh();
    //       // colliders[chunks[x, y]] = collider;
    //       MeshColliderController.AddMeshCollider(chunks[x, y], chunks[x, y].GetMesh());
    //     }
    //     else
    //     {
    //       Debug.Log("X: " + x + " Y: " + y);
    //     }
    //   }
    // }

    // Debug.Log("Moving player...");

    int highest = 0;
    for (int h = 0; h < chunks[0, 0].blockMap.GetLength(2); h++)
    {
      if (chunks[0, 0].blockMap[0, 0, h] != -1) highest = h;
    }

    player.transform.position = new Vector3(.5f, highest + 1.5f, .5f);

    Debug.Log("Map Generated!");
  }

  float[,,] GetDeepNoise(float chunkX, float chunkY)
  {
    float[,,] noise = new float[chunkSize, chunkSize, chunkHeight];
    for (int x = 0; x < chunkSize; x++)
    {
      for (int y = 0; y < chunkSize; y++)
      {
        for (int h = 0; h < chunkHeight; h++)
        {
          float xCoord = (chunkX * chunkSize) + x;
          float yCoord = (chunkY * chunkSize) + y;
          float hCoord = h;

          noise[x, y, h] = noiseFilter.Evaluate(new Vector3(xCoord * noiseScale, hCoord * noiseScale, yCoord * noiseScale));
        }
      }
    }
    return noise;
  }

  float[,] GetSurfaceNoise(float chunkX, float chunkY)
  {
    float[,] noise = new float[chunkSize, chunkSize];
    for (int x = 0; x < chunkSize; x++)
    {
      for (int y = 0; y < chunkSize; y++)
      {
        float xCoord = ((chunkX * chunkSize) + x) / worldSize * chunkSize;
        float yCoord = ((chunkY * chunkSize) + y) / worldSize * chunkSize;

        noise[x, y] = Mathf.Clamp(Mathf.PerlinNoise(seed + xCoord * surfaceNoiseScale, seed + yCoord * surfaceNoiseScale), 0, 1) * chunkHeight * surfaceNoiseStrength + minSurfaceHeight;
      }
    }
    return noise;
  }

  public void GenerateMeshes()
  {
    for (int x = 0; x < chunks.GetLength(0); x++)
    {
      for (int y = 0; y < chunks.GetLength(1); y++)
      {
        chunks[x, y].GenerateMesh();
      }
    }

    Debug.Log("Meshes Generated!");
  }

  public void ClearChunks()
  {
    for (int i = transform.childCount - 1; i >= 0; i--)
    {
      DestroyImmediate(transform.GetChild(i).gameObject);
    }

    MeshColliderController.ClearColliders();
  }

  public void BreakBlockAtPoint(Vector3 worldPoint)
  {
    Vector2 chunkPosition = WorldPointToChunk(worldPoint);
    // Debug.Log(chunkPoint);
    chunks[(int)chunkPosition.x, (int)chunkPosition.y].BreakBlockAtPoint(worldPoint);
  }

  public Vector2 WorldPointToChunk(Vector3 point)
  {
    int chunkX = (int)point.x / chunkSize;
    int chunkY = (int)point.z / chunkSize;

    return new Vector3(chunkX, chunkY);
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.black;
    Gizmos.DrawSphere(new Vector3(0, 0, 0), .1f);
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(new Vector3(1, 0, 0), .1f);
    Gizmos.color = Color.blue;
    Gizmos.DrawSphere(new Vector3(0, 0, 1), .1f);
  }
}
