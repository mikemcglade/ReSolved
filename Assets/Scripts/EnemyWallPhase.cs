using UnityEngine;

public class EnemyWallPhase : MonoBehaviour
{
    [Header("Phase Settings")]
    [Tooltip("Layer mask for walls - set this to your wall layer")]
    public LayerMask wallLayer;
    
    [Tooltip("Alpha when inside a wall (0-1, e.g. 0.6 for semi-transparent)")]
    [Range(0f, 1f)]
    public float phasingAlpha = 0.6f;
    
    [Tooltip("Alpha when in open space (usually 1.0 for fully opaque)")]
    [Range(0f, 1f)]
    public float normalAlpha = 1f;
    
    [Tooltip("How quickly the fade transitions")]
    public float fadeSpeed = 8f;

    private Renderer[] enemyRenderers;
    private Material[] originalMaterials;
    private Material[] phasingMaterials;
    private float targetAlpha;
    private float currentAlpha;

    void Start()
    {
        // Get all renderers on this enemy and its children
        enemyRenderers = GetComponentsInChildren<Renderer>();
        
        // Create material instances so we don't affect the shared material
        int totalMaterials = 0;
        foreach (var renderer in enemyRenderers)
        {
            totalMaterials += renderer.materials.Length;
        }
        
        originalMaterials = new Material[totalMaterials];
        phasingMaterials = new Material[totalMaterials];
        
        int index = 0;
        foreach (var renderer in enemyRenderers)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                // Store reference to original
                originalMaterials[index] = mats[i];
                
                // Create a copy for phasing
                phasingMaterials[index] = new Material(mats[i]);
                
                // Enable transparency rendering mode
                SetMaterialTransparent(phasingMaterials[index]);
                
                index++;
            }
        }
        
        currentAlpha = normalAlpha;
        targetAlpha = normalAlpha;
    }

    void Update()
    {
        // Check if enemy is inside a wall collider
        bool insideWall = Physics.CheckSphere(transform.position, 0.3f, wallLayer);
        
        targetAlpha = insideWall ? phasingAlpha : normalAlpha;
        
        // Smoothly lerp current alpha toward target
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        
        // Apply alpha to all materials
        ApplyAlpha(currentAlpha);
    }

    private void ApplyAlpha(float alpha)
    {
        int index = 0;
        foreach (var renderer in enemyRenderers)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Color color = phasingMaterials[index].color;
                color.a = alpha;
                phasingMaterials[index].color = color;
                
                // Update the renderer's material
                mats[i] = phasingMaterials[index];
                index++;
            }
            renderer.materials = mats;
        }
    }

    private void SetMaterialTransparent(Material mat)
    {
        // Switch to Transparent rendering mode
        mat.SetFloat("_Mode", 3); // 3 = Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    void OnDestroy()
    {
        // Clean up material instances to prevent memory leaks
        if (phasingMaterials != null)
        {
            foreach (Material mat in phasingMaterials)
            {
                if (mat != null)
                    Destroy(mat);
            }
        }
    }
}
