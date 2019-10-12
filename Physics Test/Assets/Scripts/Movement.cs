using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

  [Header("// UTILITIES")]
  public LayerMask staticLayer;

  [Space(15)]
  [Header("// VEHICLE SETTINGS")]
  public int vehicleSpeed;
  public float maxVehicleSpeed;
  public float vehicleTorque;
  public float vehicleTorqueDamp;
  public int vehicleMass;
  public int vehicleDrag;
  public Vector3 vehicleCenterOfMass;
  public AnimationCurve accelerationCurve;

  [Space(15)]
  [Header("// VEHICLE DATA")]
  public bool isGrounded;
  public bool hasMoveInput;
  public bool hasTurnInput;
  private float accelerationTime;
  public Vector3 velocity;
  public Vector3 groundNormal;
  public Vector3 lastGroundNormal;

  [Space(15)]
  [Header("// VEHICLE COMPONENTS")]
  public Rigidbody playerRB;
  public CapsuleCollider playerCollider;
  public TrailRenderer trail;

  [Space(15)]
  [Header("// VEHICLE INPUTS")]
  public Vector3 moveVector;
  public Vector3 turnVector;


  void OnEnable() {
    if (playerRB == null) {
      playerRB = gameObject.GetComponent<Rigidbody>();
    }

    if (playerCollider == null) {
      playerCollider = gameObject.GetComponentInChildren<CapsuleCollider>();
    }

    // Set defaults
    accelerationTime = 0;
    velocity = Vector3.zero;
    isGrounded = false;
    lastGroundNormal = Vector3.up;
    playerRB.mass = vehicleMass;
    playerRB.drag = vehicleDrag;
    playerRB.centerOfMass = vehicleCenterOfMass;
  }

  void OnDrawGizmosSelected() {
    // TODO: don't keep this
    Vector3 drawPosition = transform.TransformPoint(playerRB.centerOfMass);
    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(drawPosition, 1);
  }

  void Update() {
    // GetGroundData();
    SweepCapsule();

    moveVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    turnVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    hasMoveInput = moveVector != Vector3.zero;
    hasTurnInput = turnVector != Vector3.zero;

    // TODO: move this into it's own manager
    if (isGrounded) {
      if (!trail.emitting) {
        trail.emitting = true;
      }
    }
    else {
      if (trail.emitting) {
        trail.emitting = false;
      }
    }
  }

  void SweepCapsule() {
    // isGrounded
    // groundNormal
    RaycastHit[] hits;
    Vector3 frontPosition = transform.position + (transform.forward * 2);
    Vector3 backPosition = transform.position + (-transform.forward * 2);
    hits = Physics.CapsuleCastAll(frontPosition, backPosition, playerCollider.radius * 0.8f, Vector3.down, 10f, staticLayer);

    Vector3 hitNormal = Vector3.zero;
    float hitDistance = 0;
    for (int i = 0; i < hits.Length; i++) {
      RaycastHit hit = hits[i];
      // Debug.DrawRay(frontPosition, Vector3.down * hit.distance, Color.green);
      // Debug.DrawRay(backPosition, Vector3.down * hit.distance, Color.cyan);
      Debug.DrawRay(transform.position, (hit.point - transform.position) * hit.distance, Color.cyan, 0.2f);
      Debug.DrawLine(frontPosition, backPosition, Color.red);
      Debug.Log(hits.Length);
      hitNormal += hit.normal;
      hitDistance += hit.distance;
    }
    hitNormal /= hits.Length;
    hitDistance /= hits.Length;

    float distanceFromGround = hitDistance - playerCollider.radius;
    isGrounded = distanceFromGround <= playerCollider.radius;
    groundNormal = hitNormal;

    // Debug.Log(hitDistance + " " + isGrounded);
  }

  void GetGroundData() {
    // TODO: clean this shit up

    //if they dont match
    if (lastGroundNormal != groundNormal) {
      //then update the cache
      lastGroundNormal = groundNormal;
    }

    // the the front and back of vehicle
    Vector3 frontOffset = transform.forward * 2;
    Vector3 backOffset = -transform.forward * 2;

    RaycastHit rayFront;
    RaycastHit rayCenter;
    RaycastHit rayBack;
    bool hitFront = Physics.Raycast(transform.position + frontOffset, Vector3.down, out rayFront, Mathf.Infinity, staticLayer);
    bool hitCenter = Physics.Raycast(transform.position, Vector3.down, out rayCenter, Mathf.Infinity, staticLayer);
    bool hitBack = Physics.Raycast(transform.position + backOffset, Vector3.down, out rayBack, Mathf.Infinity, staticLayer);
    // Debug.DrawRay(transform.position + frontOffset, Vector3.down * 5, Color.red);
    // Debug.DrawRay(transform.position + backOffset, Vector3.down * 5, Color.blue);

    Vector3 normalFront = hitFront ? rayFront.normal : Vector3.up;
    Vector3 normalCenter = hitCenter ? rayCenter.normal : Vector3.up;
    Vector3 normalBack = hitBack ? rayBack.normal : Vector3.up;
    groundNormal = (normalFront + normalCenter + normalBack) / 3;

    float distanceFromGround = rayCenter.distance - playerCollider.radius;
    isGrounded = distanceFromGround <= playerCollider.radius;

    // smooth the groundNormal by the distanceFromGround
    groundNormal = Vector3.SmoothDamp(lastGroundNormal, groundNormal, ref velocity, distanceFromGround);
  }

  void FixedUpdate() {
    Move(moveVector, vehicleSpeed);
    RotateVehicle(turnVector);
  }

  void Move(Vector3 direction, float speed) {
    // Modify speed by input
    speed *= Acceleration(hasMoveInput);
    if (isGrounded) {
      playerRB.AddForce(direction * speed, ForceMode.Acceleration);
      ClampSpeed();
    }
  }

  void RotateVehicle(Vector3 direction) {
    // Check if input should be ignored
    if (!hasTurnInput || !isGrounded) {
      direction = transform.forward;
    }

    Vector3 projection = Vector3.ProjectOnPlane(direction, groundNormal);
    Quaternion targetRotation = Quaternion.LookRotation(projection, groundNormal);
    Quaternion deltaRotation = Quaternion.Inverse(playerRB.transform.rotation) * targetRotation;
    Vector3 deltaAngles = GetRelativeAngles(deltaRotation.eulerAngles);
    Vector3 worldDeltaAngles = transform.TransformDirection(deltaAngles);

    // Default values used: vehicleTorque = 0.025, vehicleTorqueDamp = 0.2
    // vehicleTorque controls how fast you rotate the body towards the target rotation
    // vehicleTorqueDamp prevents overshooting the target rotation
    playerRB.AddTorque((vehicleTorque * worldDeltaAngles) - (vehicleTorqueDamp * playerRB.angularVelocity));
  }

  float Acceleration(bool shouldAccelerate) {
    accelerationTime += shouldAccelerate ? Time.fixedDeltaTime : -Time.fixedDeltaTime;
    // TODO: instead of clamping, may want to find percent of desired time, this defaults to 1 sec
    accelerationTime = Mathf.Clamp01(accelerationTime); // clamp because Evaluate has a range of 0-1
    return accelerationCurve.Evaluate(accelerationTime);
  }

  void ClampSpeed() {
    if (playerRB.velocity.magnitude >= maxVehicleSpeed) {
      playerRB.velocity = Vector3.ClampMagnitude(playerRB.velocity, maxVehicleSpeed);
    }
  }

  Vector3 GetRelativeAngles(Vector3 angles) {
    // Convert angles above 180 degrees into negative/relative angles
    Vector3 relativeAngles = angles;
    if (relativeAngles.x > 180f)
      relativeAngles.x -= 360f;
    if (relativeAngles.y > 180f)
      relativeAngles.y -= 360f;
    if (relativeAngles.z > 180f)
      relativeAngles.z -= 360f;
    return relativeAngles;
  }
}
