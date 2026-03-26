using System.Collections;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform playerTransform;
    public float radius = 10f;
    public float rotationSpeed = 1.5f;
    public float rotationDuration = 5f;
    public float heightOffset = 4f;

    [Header("References")]
    public Camera mainCamera;
    public PlayerControl playerControl;

    private float currentAngle = 0f;

    void Start()
    {
        StartCoroutine(OrbitRoutine());
    }

    IEnumerator OrbitRoutine()
    {
        // Freeze player, activate orbit cam, deactivate main cam
        if (playerControl != null)
            playerControl.enabled = false;

        mainCamera.gameObject.SetActive(false);
        gameObject.SetActive(true);

        float elapsed = 0f;
        currentAngle = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float x = playerTransform.position.x + radius * Mathf.Cos(currentAngle);
            float z = playerTransform.position.z + radius * Mathf.Sin(currentAngle);
            float y = playerTransform.position.y + heightOffset;

            transform.position = new Vector3(x, y, z);
            transform.LookAt(playerTransform.position + Vector3.up * 1.5f);

            currentAngle += rotationSpeed * Time.deltaTime;

            yield return null;
        }

        // Hand back to main camera, unfreeze player
        mainCamera.gameObject.SetActive(true);
        playerControl.enabled = true;

        // Deactivate orbit camera entirely
        gameObject.SetActive(false);
    }
}
