using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFace
{
  Mesh mesh;
  int resolution;
  Vector3 localUp;
  Vector3 axisA;
  Vector3 axisB;

  Vector3 offset;
  Vector3 position;

  Vector2[] uv;

  public BlockFace(Mesh mesh, int resolution, Vector3 localUp, Vector3 position, Vector2[] uv)
  {
    this.mesh = mesh;
    this.resolution = resolution;
    this.localUp = localUp;
    this.uv = uv;

    offset = new Vector3(.5f, -.5f, .5f) + position;

    axisA = new Vector3(localUp.y, localUp.z, localUp.x);
    axisB = Vector3.Cross(localUp, axisA);
  }

  public void ConstructMesh()
  {
    Vector3[] vertices = new Vector3[resolution * resolution];
    int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
    int triIndex = 0;

    for (int y = 0; y < resolution; y++)
    {
      for (int x = 0; x < resolution; x++)
      {
        int i = x + y * resolution;
        Vector2 percent = new Vector2(x, y) / (resolution - 1);
        vertices[i] = localUp / 2 + (percent.x - .5f) * axisA + (percent.y - .5f) * axisB + offset;

        if (x != resolution - 1 && y != resolution - 1)
        {
          triangles[triIndex] = i;
          triangles[triIndex + 1] = i + resolution + 1;
          triangles[triIndex + 2] = i + resolution;

          triangles[triIndex + 3] = i;
          triangles[triIndex + 4] = i + 1;
          triangles[triIndex + 5] = i + resolution + 1;
          triIndex += 6;
        }
      }
    }

    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uv;
    mesh.RecalculateNormals();
  }
}
