using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Utilities : MonoBehaviour {

  public static Game_Utilities instance = null;
  public Camera_Controller cameraController;
  public GameObject player;
  public Transform forwardReference;
  public float circleFunction = 0.7071f;
  public LayerMask staticLayer;

  void Awake() {
    if (instance == null)
      instance = this;
    else if (instance != this)
      Destroy(gameObject);

    DontDestroyOnLoad(gameObject);

    if (cameraController == null) {
      cameraController = GameObject.FindGameObjectWithTag("CameraController").GetComponent<Camera_Controller>();
    }

    if (player == null) {
      // TODO: spawn as prefab instead
      player = GameObject.FindGameObjectWithTag("Player");
    }
  }

  void Update() {
    ChangeControls();
  }

  void ChangeControls() {
    // poll for settings updates updates
    if (Game_Settings.instance.twinStickControls) {
      // controls are relative to the camera
      forwardReference = cameraController.currentCamera;
    }
    else {
      // controls are relative to the player
      forwardReference = player.transform;
    }
  }

  public Vector3 ConvertInputForISO(Vector3 direction) {
    Vector3 currentForward = forwardReference.eulerAngles;

    // the only axis that matters is Y, reset the others
    currentForward.x = 0;
    currentForward.z = 0;

    // snap to 45 degree increments
    if (!Game_Settings.instance.twinStickControls) {
      currentForward.y = Mathf.Round(currentForward.y / (45.0f * 0.5f)) * (45.0f * 0.5f);
    }
    Vector3 newVector = Quaternion.Euler(currentForward) * direction;
    newVector = newVector.normalized;

    return newVector;
  }
}
