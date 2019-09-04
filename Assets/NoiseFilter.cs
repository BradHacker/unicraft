using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter
{
  NoiseSettings settings;
  Noise noise;

  public NoiseFilter(int seed, NoiseSettings settings)
  {
    noise = new Noise(seed);
    this.settings = settings;
  }

  public float Evaluate(Vector3 point)
  {
    float noiseValue = 0;
    float frequency = settings.baseRoughness;
    float amplitude = 1;

    for (int i = 0; i < settings.numLayers; i++)
    {
      float v = PerlinNoise3D.Evaluate(point * settings.scale * frequency + settings.center);
      noiseValue += (v + 1) * .5f * amplitude;
      frequency *= settings.roughness;
      amplitude *= settings.persistence;
    }
    return noiseValue * settings.strength;
  }
}
