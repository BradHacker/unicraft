using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
  public static Vector2[] baseUv = { new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 1f) };
  public static Vector2[] rotatedUv = { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f) };

  public static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
  public static Vector2[][] faceUvMaps = { baseUv, baseUv, rotatedUv, rotatedUv, baseUv, baseUv, };

  public static Dictionary<int, bool> transparencyAtlas = new Dictionary<int, bool>()
  {
      { 3, true }
  };
}
