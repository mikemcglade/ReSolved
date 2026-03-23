using UnityEngine;

public class PowerupAnimation : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Degrees per second on the Y axis")]
    public float rotationSpeed = 90f;

    [Header("Float")]
    [Tooltip("How far up and down the object bobs")]
    public float floatHeight = 0.15f;
    [Tooltip("How fast the object bobs")]
    public float floatSpeed = 1.2f;

    [Header("Pulse")]
    [Tooltip("How much the scale breathes in and out (keep small, e.g. 0.08)")]
    public float pulseAmount = 0.08f;
    [Tooltip("How fast the pulse cycles")]
    public float pulseSpeed = 1.8f;

    private Vector3 startPosition;
    private Vector3 startScale;

    // Randomise phase per instance so multiple powerups don't pulse in sync
    private float floatOffset;
    private float pulseOffset;

    void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;

        // Random phase offsets so nearby pickups feel independent
        floatOffset = Random.Range(0f, Mathf.PI * 2f);
        pulseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // --- Rotation ---
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        // --- Float ---
        float newY = startPosition.y + Mathf.Sin((Time.time * floatSpeed) + floatOffset) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // --- Pulse scale ---
        float pulse = 1f + Mathf.Sin((Time.time * pulseSpeed) + pulseOffset) * pulseAmount;
        transform.localScale = startScale * pulse;
    }
}
