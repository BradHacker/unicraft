using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  public Vector3 size = new Vector3(1, 1.8f, .75f);
  public float diameter = 1;
  public float height = 1.8f;
  // public Vector3 location = new Vector3(0, 0, 0);
  // Vector3 center;

  public Rigidbody playerRigidbody;
  BoxCollider boxCollider;

  public KeyCode moveForward;
  public KeyCode moveBackward;
  public KeyCode moveLeft;
  public KeyCode moveRight;
  public KeyCode moveJump;
  public int breakBlockButton;

  public float movementVelocity = 5;
  float movementForce;
  public float jumpVelocity = 5;
  float jumpForce;
  public float rotationVelocity = 2;

  public float velocityLimit = 10;

  public float jumpRayMaxDist = 1.5f;
  public float maxInterationDistance = 8;

  private void OnValidate()
  {
    Initialize();
  }

  void Initialize()
  {
    // center = location + location + new Vector3(0, 0, 0);

    // Add components
    if (gameObject.GetComponent<BoxCollider>() == null)
    {
      gameObject.AddComponent<BoxCollider>();
    }
    if (gameObject.GetComponent<Rigidbody>() == null)
    {
      playerRigidbody = gameObject.AddComponent<Rigidbody>();
    }

    boxCollider = gameObject.GetComponent<BoxCollider>();
    boxCollider.size = size;
    // boxCollider.material = MeshColliderController.physicMaterial;
    // gameObject.GetComponent<CapsuleCollider>().center = transform.position;

    movementForce = .5f * playerRigidbody.mass * movementVelocity * movementVelocity;
    jumpForce = .5f * playerRigidbody.mass * jumpVelocity * jumpVelocity;
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      GameState.paused = !GameState.paused;

      if (GameState.paused)
      {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
      }
      else
      {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
      }
    }

    if (!GameState.paused)
    {
      gameObject.transform.Rotate(0, Input.GetAxis("Mouse X") * rotationVelocity, 0);

      if (Input.GetKey(moveForward))
      {
        playerRigidbody.MovePosition(transform.position + transform.forward * movementVelocity);
      }
      if (Input.GetKey(moveBackward))
      {
        playerRigidbody.transform.position += transform.forward * -movementVelocity;
      }

      if (Input.GetKey(moveLeft))
      {
        playerRigidbody.transform.position += transform.right * -movementVelocity;
      }
      if (Input.GetKey(moveRight))
      {
        playerRigidbody.transform.position += transform.right * movementVelocity;
      }

      if (Input.GetKeyDown(moveJump) && IsOnGround())
      {
        playerRigidbody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
      }

      if (playerRigidbody.velocity.magnitude > velocityLimit)
      {
        playerRigidbody.AddForce(-(playerRigidbody.velocity - new Vector3(velocityLimit, velocityLimit, velocityLimit)));
      }

      if (Input.GetMouseButtonDown(breakBlockButton))
      {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, maxInterationDistance))
        {
          if (hit.point != null)
          {
            // Debug.Log(hit.point);
            World world = GameObject.Find("World").GetComponent<World>();
            world.BreakBlockAtPoint(hit.point);
          }
        }
      }
    }
  }

  bool IsOnGround()
  {
    Vector3 frontRight = playerRigidbody.transform.position + (playerRigidbody.transform.forward * size.z / 2) + (playerRigidbody.transform.right * size.x / 2);
    Vector3 frontLeft = playerRigidbody.transform.position + (playerRigidbody.transform.forward * size.z / 2) + (-playerRigidbody.transform.right * size.x / 2);
    Vector3 backRight = playerRigidbody.transform.position + (-playerRigidbody.transform.forward * size.z / 2) + (playerRigidbody.transform.right * size.x / 2);
    Vector3 backLeft = playerRigidbody.transform.position + (-playerRigidbody.transform.forward * size.z / 2) + (-playerRigidbody.transform.right * size.x / 2);
    return Physics.Raycast(frontRight, -Vector3.up, jumpRayMaxDist) || Physics.Raycast(frontLeft, -Vector3.up, jumpRayMaxDist) || Physics.Raycast(backRight, -Vector3.up, jumpRayMaxDist) || Physics.Raycast(backLeft, -Vector3.up, jumpRayMaxDist);
  }

  // public void OnDrawGizmos()
  // {
  //   Vector3 frontRight = playerRigidbody.transform.position + (playerRigidbody.transform.forward * size.z / 2) + (playerRigidbody.transform.right * size.x / 2);
  //   Gizmos.DrawSphere(frontRight, .5f);
  // }
}
