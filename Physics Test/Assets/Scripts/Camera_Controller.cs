using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Camera_Controller : MonoBehaviour {

  public Transform currentCamera;
  public Transform mainCamera;
  public List<GameObject> topDownCameras = new List<GameObject>();
  public GameObject perspectiveCameraPrefab;
  public GameObject isometricCameraPrefab;
  public GameObject isometricParent;
  public GameObject perspectiveParent;
  public Vector3[] isometricAngles;
  public Transform followTarget;

  void OnEnable() {

    if (mainCamera == null) {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    PrepCameras();
  }

  void PrepCameras() {
    if (Game_Settings.instance.isometricCamera) {
      // make sure iso is on
      mainCamera.gameObject.GetComponent<Camera>().orthographic = true;
      isometricParent.SetActive(true);

      // create kiddos if needed
      if (isometricParent.transform.childCount == 0) {
        CreateIsoCams();
      }

      // set the current active cam
      isometricParent.transform.GetChild(0).gameObject.SetActive(true);
      currentCamera = isometricParent.transform.GetChild(0);

      // make sure perspective is off
      perspectiveParent.SetActive(false);
    }
    else {
      // make sure perspective is on
      mainCamera.gameObject.GetComponent<Camera>().orthographic = false;
      perspectiveParent.SetActive(true);

      // create kiddos if needed
      if (perspectiveParent.transform.childCount == 0) {
        CreatePerspectiveCam();
      }

      // set the current active cam
      perspectiveParent.transform.GetChild(0).gameObject.SetActive(true);
      currentCamera = perspectiveParent.transform.GetChild(0);

      // make sure iso is off
      isometricParent.SetActive(false);
    }
  }

  void CreatePerspectiveCam() {
    GameObject camera = Instantiate(perspectiveCameraPrefab, transform.position, Quaternion.identity, perspectiveParent.transform);
    SetCameraProperties(camera, Game_Utilities.instance.player.transform);
  }

  void CreateIsoCams() {
    for (int i = 0; i < isometricAngles.Length; i++) {
      Quaternion rotation = Quaternion.Euler(isometricAngles[i]);
      GameObject camera = Instantiate(isometricCameraPrefab, transform.position, rotation, isometricParent.transform);
      SetCameraProperties(camera, Game_Utilities.instance.player.transform);
      topDownCameras.Add(camera.gameObject);
      camera.SetActive(false);
    }
  }

  void SetCameraProperties(GameObject camera, Transform target) {
    camera.GetComponent<CinemachineVirtualCamera>().Follow = target;
  }

  void Update() {
    if (Game_Settings.instance.isometricCamera) {
      ChangeCamera();
    }

    if (Input.GetKeyDown("o") || Input.GetKeyDown("p")) {
      // TODO: fix this when setting up rewired
      PrepCameras();
    }
  }

  void ChangeCamera() {
    int lastCamera = topDownCameras.Count - 1;
    if (Input.GetKeyDown("q")) {
      for (int i = 0; i < topDownCameras.Count; i++) {
        if (topDownCameras[i].activeInHierarchy) {
          topDownCameras[i].SetActive(false);

          // go to the end of the list or select previous camera
          int previousCamera = i == 0 ? lastCamera : i - 1;
          topDownCameras[previousCamera].SetActive(true);
          currentCamera = topDownCameras[previousCamera].transform;
          break;
        }
      }
    }
    else if (Input.GetKeyDown("e")) {
      for (int i = 0; i < topDownCameras.Count; i++) {
        if (topDownCameras[i].activeInHierarchy) {
          topDownCameras[i].SetActive(false);

          // go to the start of the list or select next camera
          int nextCamera = i == lastCamera ? 0 : i + 1;
          topDownCameras[nextCamera].SetActive(true);
          currentCamera = topDownCameras[nextCamera].transform;
          break;
        }
      }
    }
  }
}
