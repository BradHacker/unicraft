using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
  [Range(8, 16)]
  public int chunkSize = 8;
  [Range(1, 100)]
  public int chunkHeight = 8;
  public int worldSize = 1;
  public int seed;

  Chunk[,] chunks;
  public Player player;

  void Start()
  {
    player = GameObject.Find("Player").GetComponent<Player>();
    // noise = new SimplexNoiseGenerator(seed);
  }

  public void Generate()
  {
    // if (player == null) player = GameObject.Find("Player").GetComponent<Player>();
    // if (noise == null) noise = new SimplexNoiseGenerator(seed);

    Debug.Log("Starting Map Generation...");

    if (transform.childCount > 0) ClearChunks();

    // Debug.Log("Generating new chunks...");
    chunks = new Chunk[worldSize, worldSize];

    for (int y = 0; y < worldSize; y++)
    {
      for (int x = 0; x < worldSize; x++)
      {
        // Debug.Log("Chunk " + (y * worldSize + x) + "/" + (worldSize * worldSize) + " loaded...");
        chunks[x, y] = new Chunk(chunkSize, chunkHeight, new Vector2(x, y), transform, GetNoise(x, y), seed);
      }
    }

    // Debug.Log("Moving player...");

    int highest = 0;
    for (int h = 0; h < chunks[0, 0].blockMap.GetLength(2); h++)
    {
      if (chunks[0, 0].blockMap[0, 0, h] != 0) highest = h;
    }

    player.transform.position = new Vector3(.5f, highest + .9f, .5f);

    Debug.Log("Map Generated!");

    GenerateMeshes();
  }

  float[,] GetNoise(float chunkX, float chunkY)
  {
    float[,] noise = new float[chunkSize, chunkSize];
    for (int y = 0; y < chunkSize; y++)
    {
      for (int x = 0; x < chunkSize; x++)
      {
        float xCoord = ((chunkX * chunkSize) + x) / (worldSize * chunkSize);
        float yCoord = ((chunkY * chunkSize) + y) / (worldSize * chunkSize);

        noise[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
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
