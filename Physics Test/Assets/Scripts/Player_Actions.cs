using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Actions : MonoBehaviour {

  public Camera mainCamera;
  void OnEnable() {
    if (mainCamera == null) {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
  }

  void Update() {
    // TODO: uh of time for rewired

    // Vector3 point = new Vector3();
    // // Event currentEvent = Event.current;
    // Vector2 mousePos = new Vector2();

    // // Get the mouse position from Event.
    // // Note that the y position from Event is inverted.
    // // mousePos.x = currentEvent.mousePosition.x;
    // // mousePos.y = mainCamera.pixelHeight - currentEvent.mousePosition.y;
    // // mousePos.x = Input.Ge

    // point = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));

    // Debug.DrawRay(transform.position, transform.position - point, Color.blue);
  }
}
