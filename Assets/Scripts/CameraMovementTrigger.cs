using UnityEngine;

public class CameraMovementTrigger : MonoBehaviour
{
    public CircularCameraMovement cameraMovementScript;

    void Start()
    {
        // Enable the CircularCameraMovement script on the camera
        cameraMovementScript.enabled = true;
    }
}