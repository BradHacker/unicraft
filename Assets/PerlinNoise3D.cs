using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise3D
{
  public static float Evaluate(Vector3 point)
  {
    return GetNoise(point.x, point.y, point.z);
  }

  public static float Evaluate(float x, float y, float z)
  {
    return GetNoise(x, y, z);
  }

  static float GetNoise(float x, float y, float z)
  {
    float AB = Mathf.PerlinNoise(x, y);
    float BC = Mathf.PerlinNoise(y, z);
    float AC = Mathf.PerlinNoise(x, z);

    float BA = Mathf.PerlinNoise(y, x);
    float CB = Mathf.PerlinNoise(z, y);
    float CA = Mathf.PerlinNoise(z, x);

    float ABC = AB + BC + AC + BA + CB + CA;
    return ABC / 6f;
  }
}
