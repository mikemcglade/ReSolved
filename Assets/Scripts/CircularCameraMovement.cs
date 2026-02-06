using UnityEngine;

public class CircularCameraMovement : MonoBehaviour
{
    public Transform playerTransform;
    public float radius = 10f;
    public float rotationSpeed = 0.5f; // Reduced speed for a slower rotation
    public float rotationDuration = 5f; // Duration of the rotation

    private float currentAngle = 0f;
    private float elapsedTime = 0f;
   private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;



    void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime < rotationDuration)
        {
            // Calculate the desired position around the player
            float x = playerTransform.position.x + radius * Mathf.Cos(currentAngle);
            float z = playerTransform.position.z + radius * Mathf.Sin(currentAngle);

            // Set the camera's position
            transform.position = new Vector3(x, transform.position.y, z);

            // Look at the player
            transform.LookAt(playerTransform);

            // Increment the angle for the next frame
            currentAngle += rotationSpeed * Time.deltaTime;

        }
        else
        {
            // Reset the camera's local position and rotation
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
            
            // Disable the script after the rotation is complete
            enabled = false;
        }
    }
}