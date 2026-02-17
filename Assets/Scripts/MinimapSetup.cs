using UnityEngine;
using UnityEngine.UI;

public class MinimapSetup : MonoBehaviour
{
    [Tooltip("Assign the player dot RectTransform")]
    public RectTransform playerDot;

    // Player dot always stays at centre since the camera follows the player
    // This script exists as a hook for future expansion (enemy dots etc)
    // and to confirm the dot is correctly centred at start

    void Start()
    {
        if (playerDot != null)
        {
            playerDot.anchoredPosition = Vector2.zero;
        }
    }
}
