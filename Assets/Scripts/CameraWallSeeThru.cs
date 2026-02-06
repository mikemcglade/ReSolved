using UnityEngine;

public class CameraWallHack : MonoBehaviour
{
    public Transform player;
    public LayerMask wallLayer;
    public float fadeSpeed = 2f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 direction = player.position - transform.position;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, direction.magnitude, wallLayer);

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                Color color = material.color;
                color.a = Mathf.Lerp(color.a, 0.2f, Time.deltaTime * fadeSpeed);
                material.color = color;
            }
        }
    }
}