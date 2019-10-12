using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumping : MonoBehaviour {

  public float t;
  public Vector3 pos;
  public Vector3 speed;
  public float speedJump;
  public float gravity;

  public float t0;
  public Vector3 pos0;
  public Vector3 speed0;

  public bool isJumping;

  void Start() {
    gravity = Physics2D.gravity.magnitude;
  }

  void Update() {
    Jump();
  }

  void Jump() {
    // Vectors
    // pos0: The position of the character when he begin to jump.
    // speed0: The speed of the character when he begin to jump.
    // pos: current position
    // speed: current speed

    // Values:
    // g: gravity.
    // t0: time when the character begin to jump.
    // t: time elapsed in the current jump.
    // speedJump: The momentum added by jumping(vertical)(constant)
    // isJumping: if your character is currently jumping

    // When the player press the space key, your hold the t0 and pos0, and compute speed0:
    if (!isJumping && Input.GetButton("Jump")) {
      t0 = Time.time;
      pos0 = pos;
      speed0 = speed;
      speed0.y += speedJump;
      isJumping = true;
    }

    // Every frame, you may compute the new position of the character with:
    if (isJumping) {
      t = Time.time - t0;
      pos.y = pos0.y + speed0.y * t - gravity * t * t;
      pos.x = pos0.x + speed0.x * t;

      transform.position = pos;

      // And test that the character is not on the ground again.
      if (pos.y < 0) {
        pos.y = 0;
        isJumping = false;
      }
    }
  }
}
