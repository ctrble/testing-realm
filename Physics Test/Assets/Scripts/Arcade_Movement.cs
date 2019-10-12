using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arcade_Movement : MonoBehaviour {

  [Header("Input")]
  public Vector3 moveVector;
  public Vector3 lastVector;

  [Header("Utilities")]
  public LayerMask staticLayer;

  [Header("Vehicle Components")]
  public Rigidbody vehicleRB;
  public CapsuleCollider vehicleCollider;
  public Transform tiresParent;
  public List<Transform> tires = new List<Transform>();
  public Transform trailsParent;
  public List<TrailRenderer> trails = new List<TrailRenderer>();

  [Header("Vehicle Settings")]
  public float gravityForce;
  public float hoverForce;
  public float hoverHeight;
  public float vehicleSpeed;
  public float currentThrust;
  public float maxVelocity;
  public float groundedDrag;
  public AnimationCurve accelerationCurve;
  public float accelerationPercent;
  public float vehicleTorque;
  public float vehicleTorqueDamp;
  public List<Vector3> hoverDirection = new List<Vector3>();

  [Header("Vehicle Info / Debug")]
  public bool grounded;
  public Vector3 groundNormal;
  public float groundFriction;

  void OnEnable() {
    if (vehicleRB == null) {
      vehicleRB = gameObject.GetComponent<Rigidbody>();
    }
    if (vehicleCollider == null) {
      vehicleCollider = gameObject.GetComponentInChildren<CapsuleCollider>();
    }

    // Better hovering stability to include the middle
    tires.Add(tiresParent);
    hoverDirection.Add(transform.position);
    foreach (Transform child in tiresParent) {
      tires.Add(child);
      hoverDirection.Add(child.position);
    }

    foreach (Transform child in trailsParent) {
      trails.Add(child.GetComponent<TrailRenderer>());
    }

    vehicleRB.centerOfMass = Vector3.down;
    // vehicleRB.centerOfMass = new Vector3(0, -1, -1);
    hoverHeight = tiresParent.position.y;


    gravityForce = Physics.gravity.y * 100f;
    hoverForce = -Physics.gravity.y * 10f;

    // Set intelligent "last" value
    lastVector = transform.forward;
  }

  void Update() {
    GetInput();
    GroundData();
    TireTrails();
  }

  void GetInput() {
    moveVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    moveVector = Game_Utilities.instance.ConvertInputForISO(moveVector);

    // Set previous vector when needed, omit zero values
    if (moveVector != lastVector && moveVector != Vector3.zero) {
      lastVector = moveVector;
    }
  }

  void FixedUpdate() {
    BalanceVehicle();
    AdjustDragAndThrust(moveVector);
    TurnVehicle(moveVector);
    MoveVehicle(moveVector, currentThrust);
  }

  void GroundData() {
    RaycastHit hit;
    grounded = false;
    Vector3 tempGroundNormal = Vector3.zero;

    for (int i = 0; i < tires.Count; i++) {
      Transform hoverPoint = tires[i];
      if (Physics.Raycast(hoverPoint.position, Vector3.down, out hit, hoverHeight, staticLayer)) {
        // Maintain level
        hoverDirection[i] = Vector3.up * hoverForce * (1.0f - (hit.distance / hoverHeight));
        tempGroundNormal += hit.normal.normalized;
        grounded = true;

        // really only care about the middle of the vehicle
        if (i == 0) {
          // this is a super mess but it kinda works
          // TODO: fix it cache it bake it in a stew
          groundFriction = hit.transform.gameObject.GetComponent<MeshCollider>().material.dynamicFriction;
        }
      }
      else {
        // Level the vehicle when not grounded and simulate gravity
        float difference = (1.0f - (transform.position.y - hoverPoint.position.y / hoverHeight));
        hoverDirection[i] = grounded ? (Vector3.up * hoverForce * difference) : (Vector3.up * gravityForce);
        tempGroundNormal += Vector3.up;
      }
    }

    // Average the normals, then verify and normalize
    groundNormal = tempGroundNormal / tires.Count;
    groundNormal = groundNormal == Vector3.zero ? Vector3.up : groundNormal.normalized;
  }

  void BalanceVehicle() {
    for (int i = 0; i < tires.Count; i++) {
      vehicleRB.AddForceAtPosition(hoverDirection[i], tires[i].position);
    }
  }

  void AdjustDragAndThrust(Vector3 direction) {
    bool hasInput = direction != Vector3.zero;
    currentThrust = vehicleSpeed * Acceleration(hasInput);

    if (grounded) {
      float frictionPercent = Mathf.Clamp01(groundFriction);
      float newDrag = (groundedDrag * frictionPercent);
      vehicleRB.drag = newDrag;

      // if (frictionPercent < 1) {
      //   // TODO: this is broke do fix it
      //   Debug.Log(moveVector + " " + lastVector);
      //   moveVector += (lastVector);
      // }
    }
    else {
      vehicleRB.drag = 0.1f;
      currentThrust /= 100f;
    }
  }

  float Acceleration(bool shouldAccelerate) {
    accelerationPercent += shouldAccelerate ? Time.fixedDeltaTime : -Time.fixedDeltaTime;
    accelerationPercent = Mathf.Clamp01(accelerationPercent);
    return accelerationCurve.Evaluate(accelerationPercent);
  }

  void MoveVehicle(Vector3 direction, float speed) {
    if (speed != 0) {
      Vector3 projectedVector = Vector3.ProjectOnPlane(direction, groundNormal);
      vehicleRB.AddForce(projectedVector * speed, ForceMode.Acceleration);
    }

    // Limit max velocity
    if (vehicleRB.velocity.sqrMagnitude > (vehicleRB.velocity.normalized * maxVelocity).sqrMagnitude) {
      vehicleRB.velocity = vehicleRB.velocity.normalized * maxVelocity;
    }
  }

  void TurnVehicle(Vector3 direction) {
    // Snap to the last input if needed
    if (direction == Vector3.zero) {
      direction = lastVector;
    }

    // Don't accept new input while in the air
    if (!grounded) {
      direction = transform.forward;
    }


    Vector3 projection = Vector3.ProjectOnPlane(direction, groundNormal);
    Quaternion targetRotation = Quaternion.LookRotation(projection, groundNormal);
    Quaternion deltaRotation = Quaternion.Inverse(vehicleRB.transform.rotation) * targetRotation;
    Vector3 deltaAngles = GetRelativeAngles(deltaRotation.eulerAngles);
    Vector3 worldDeltaAngles = transform.TransformDirection(deltaAngles);

    float currentTorque = vehicleTorque;
    if (!Game_Settings.instance.twinStickControls) {
      currentTorque = vehicleTorque * 0.75f;
    }

    // Default values used: vehicleTorque = 0.025, vehicleTorqueDamp = 0.2
    // vehicleTorque controls how fast you rotate the body towards the target rotation
    // vehicleTorqueDamp prevents overshooting the target rotation
    vehicleRB.AddTorque((currentTorque * worldDeltaAngles) - (vehicleTorqueDamp * vehicleRB.angularVelocity));
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

  void TireTrails() {
    for (int i = 0; i < trails.Count; i++) {
      if (grounded) {
        if (!trails[i].emitting) {
          trails[i].emitting = true;
        }
      }
      else {
        if (trails[i].emitting) {
          trails[i].emitting = false;
        }
      }
    }
  }
}
