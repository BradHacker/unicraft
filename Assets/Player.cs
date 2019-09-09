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

  public float movementVelocity = 5;
  float movementForce;
  public float jumpVelocity = 5;
  float jumpForce;
  public float rotationVelocity = 2;

  public float velocityLimit = 10;

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
      // Vector3 rotateVector = new Vector3(0, 0, 0);
      // rotateVector.y += Input.GetAxis("Mouse X");
      // playerRigidbody.AddTorque(0, Input.GetAxis("Mouse X"), 0);
      gameObject.transform.Rotate(0, Input.GetAxis("Mouse X") * rotationVelocity, 0);

      if (Input.GetKey(moveForward))
      {
        playerRigidbody.MovePosition(transform.position + transform.forward * movementVelocity);
      }
      if (Input.GetKey(moveBackward))
      {
        playerRigidbody.transform.position += transform.forward * -movementVelocity;
      }
      // if (Input.GetKeyUp(moveForward) || Input.GetKeyUp(moveBackward))
      // {
      //   playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, playerRigidbody.velocity.y, 0);
      // }

      if (Input.GetKey(moveLeft))
      {
        playerRigidbody.transform.position += transform.right * -movementVelocity;
      }
      if (Input.GetKey(moveRight))
      {
        playerRigidbody.transform.position += transform.right * movementVelocity;
      }
      // if (Input.GetKeyUp(moveLeft) || Input.GetKey(moveRight))
      // {
      //   playerRigidbody.velocity = new Vector3(0, playerRigidbody.velocity.y, playerRigidbody.velocity.z);
      // }

      if (Input.GetKeyDown(moveJump) && IsOnGround())
      {
        playerRigidbody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
      }

      if (playerRigidbody.velocity.magnitude > velocityLimit)
      {
        playerRigidbody.AddForce(-(playerRigidbody.velocity - new Vector3(velocityLimit, velocityLimit, velocityLimit)));
      }

      // if (playerRigidbody.velocity.magnitude > movementVelocity)
      // {
      //   Vector3 clampedVelocity = playerRigidbody.velocity.normalized * movementVelocity;
      //   playerRigidbody.velocity = clampedVelocity;
      // }
    }
  }

  bool IsOnGround()
  {
    Vector3 frontRight = playerRigidbody.transform.position + (playerRigidbody.transform.forward * size.z / 2) + (playerRigidbody.transform.right * size.x / 2);
    Vector3 frontLeft = playerRigidbody.transform.position + (playerRigidbody.transform.forward * size.z / 2) + (-playerRigidbody.transform.right * size.x / 2);
    Vector3 backRight = playerRigidbody.transform.position + (-playerRigidbody.transform.forward * size.z / 2) + (playerRigidbody.transform.right * size.x / 2);
    Vector3 backLeft = playerRigidbody.transform.position + (-playerRigidbody.transform.forward * size.z / 2) + (-playerRigidbody.transform.right * size.x / 2);
    return Physics.Raycast(frontRight, -Vector3.up, 1) || Physics.Raycast(frontLeft, -Vector3.up, 1) || Physics.Raycast(backRight, -Vector3.up, 1) || Physics.Raycast(backLeft, -Vector3.up, 1);
  }

  // public void OnDrawGizmos()
  // {
  //   Vector3 frontRight = playerRigidbody.transform.position + (playerRigidbody.transform.forward * size.z / 2) + (playerRigidbody.transform.right * size.x / 2);
  //   Gizmos.DrawSphere(frontRight, .5f);
  // }
}
