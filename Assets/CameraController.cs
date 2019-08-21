using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  public GameObject player;
  Vector3 offset;
  float rotationVelocity;

  // Start is called before the first frame update
  void Start()
  {
    transform.position = player.transform.position + new Vector3(0, .72f, 0);
    offset = transform.position - player.transform.position;
    rotationVelocity = player.GetComponent<Player>().rotationVelocity;
  }

  // Update is called once per frame
  void Update()
  {
    transform.position = player.transform.position + offset;
    if (transform.rotation.x > -90 && transform.rotation.x < 90)
    {
      transform.Rotate(Input.GetAxis("Mouse Y") * -rotationVelocity, 0, 0);
    }
    transform.Rotate(0, Input.GetAxis("Mouse X") * rotationVelocity, 0, Space.World);
  }
}
