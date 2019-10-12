using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flying_Maybe : MonoBehaviour {

  public int playerSpeed;
  public float currentSpeed;
  public int maxPlayerSpeed;
  public int playerTorque;
  public Rigidbody playerRB;
  public Vector3 moveVector;
  public Vector3 turnVector;
  public AnimationCurve curve;
  private float accelerationTime;
  public float accelerationCurve;
  public float acceleration;
  // public bool acceleration;
  // public bool force;
  // public bool impulse;
  // public bool velocityChange;

  void OnEnable() {
    accelerationTime = 0;
  }

  void Update() {
    moveVector = new Vector3(0, 0, Input.GetAxis("Vertical"));
    turnVector = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
  }

  void FixedUpdate() {

    currentSpeed = playerRB.velocity.z;

    // F = m * a,
    // where "F" is force, "m" is mass and "a" is acceleration.
    // F = m * (v / t), where "m" is the mass of the object, "v" is the desired velocity and t = Time.fixedDeltaTime.
    // float force = playerRB.mass * (maxPlayerSpeed / Time.fixedDeltaTime);
    // playerRB.AddRelativeForce(moveVector * force, ForceMode.Acceleration);

    // track acceleration curve
    if (moveVector != Vector3.zero) {
      accelerationTime += Time.fixedDeltaTime;
    }
    else {
      accelerationTime -= Time.fixedDeltaTime;
    }

    // apply acceleration curve
    accelerationTime = Mathf.Clamp01(accelerationTime);
    acceleration = curve.Evaluate(accelerationTime);

    Vector3 direction = new Vector3(0, 0, acceleration * playerSpeed * moveVector.z);
    playerRB.AddRelativeForce(direction, ForceMode.Acceleration);

    // float acceleration = curve.Evaluate(playerSpeed * Time.fixedDeltaTime);

    // float targetSpeed = Input.GetAxis("Vertical") * playerSpeed;
    // float speedDifference = targetSpeed - currentSpeed;
    // speedDifference = Mathf.Clamp(speedDifference, -maxPlayerSpeed, maxPlayerSpeed);

    // playerRB.AddRelativeForce(new Vector3(0, 0, speedDifference), ForceMode.Acceleration);



    // this works but could be better
    // if (acceleration) {
    //   playerRB.AddRelativeForce(moveVector * playerSpeed, ForceMode.Acceleration);
    //   playerRB.AddRelativeTorque(transform.up * turnVector.x * playerTorque, ForceMode.Acceleration);
    // }
    // else if (force) {
    //   playerRB.AddRelativeForce(moveVector * playerSpeed, ForceMode.Force);
    //   playerRB.AddRelativeTorque(transform.up * turnVector.x * playerTorque, ForceMode.Force);
    // }
    // else if (impulse) {
    //   playerRB.AddRelativeForce(moveVector * playerSpeed, ForceMode.Impulse);
    //   playerRB.AddRelativeTorque(transform.up * turnVector.x * playerTorque, ForceMode.Impulse);
    // }
    // else if (velocityChange) {
    //   playerRB.AddRelativeForce(moveVector * playerSpeed, ForceMode.VelocityChange);
    //   playerRB.AddRelativeTorque(transform.up * turnVector.x * playerTorque, ForceMode.VelocityChange);
    // }
  }
}
