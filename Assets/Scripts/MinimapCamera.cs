using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [Tooltip("Assign the Player transform")]
    public Transform player;

    [Tooltip("How high above the maze the minimap camera sits")]
    public float height = 50f;

    void LateUpdate()
    {
        if (player == null) return;

        // Follow player X and Z, stay at fixed height above
        transform.position = new Vector3(player.position.x, height, player.position.z);
    }
}
