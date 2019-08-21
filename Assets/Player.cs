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
    // gameObject.GetComponent<CapsuleCollider>().center = transform.position;

    movementForce = .5f * playerRigidbody.mass * movementVelocity * movementVelocity;
    jumpForce = .5f * playerRigidbody.mass * jumpVelocity * jumpVelocity;
  }

  void Update()
  {
    // Vector3 rotateVector = new Vector3(0, 0, 0);
    // rotateVector.y += Input.GetAxis("Mouse X");
    // playerRigidbody.AddTorque(0, Input.GetAxis("Mouse X"), 0);
    gameObject.transform.Rotate(0, Input.GetAxis("Mouse X") * rotationVelocity, 0);

    if (Input.GetKey(moveForward))
    {
      playerRigidbody.velocity += transform.forward * movementVelocity;
    }
    if (Input.GetKey(moveBackward))
    {
      playerRigidbody.velocity += transform.forward * -movementVelocity;
    }
    if (Input.GetKeyUp(moveForward) || Input.GetKeyUp(moveBackward))
    {
      playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, playerRigidbody.velocity.y, 0);
    }

    if (Input.GetKey(moveLeft))
    {
      playerRigidbody.velocity += transform.right * -movementVelocity;
    }
    if (Input.GetKey(moveRight))
    {
      playerRigidbody.velocity += transform.right * movementVelocity;
    }
    if (Input.GetKeyUp(moveLeft) || Input.GetKey(moveRight))
    {
      playerRigidbody.velocity = new Vector3(0, playerRigidbody.velocity.y, playerRigidbody.velocity.z);
    }

    if (Input.GetKeyDown(moveJump))
    {
      playerRigidbody.velocity += transform.up * jumpVelocity;
    }

    if (playerRigidbody.velocity.magnitude > movementVelocity)
    {
      Vector3 clampedVelocity = playerRigidbody.velocity.normalized * movementVelocity;
      playerRigidbody.velocity = clampedVelocity;
    }
  }
}
