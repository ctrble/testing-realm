using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Settings : MonoBehaviour {

  public static Game_Settings instance = null;
  public bool twinStickControls;
  public bool isometricCamera;

  void Awake() {
    if (instance == null)
      instance = this;
    else if (instance != this)
      Destroy(gameObject);

    DontDestroyOnLoad(gameObject);
  }

  void Update() {
    if (Input.GetKeyDown("o")) {
      isometricCamera = !isometricCamera;
    }

    if (Input.GetKeyDown("p")) {
      twinStickControls = !twinStickControls;
    }

    if (!isometricCamera) {
      twinStickControls = false;
    }
  }
}
