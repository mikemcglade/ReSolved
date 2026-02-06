using UnityEngine;
using System.Collections;

public class PlayerShrink : MonoBehaviour
{
    [SerializeField] private Vector3 shrunkScale = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private float shrinkDuration = 0.1f;
    [SerializeField] private float shrunkDuration = 5.0f;

    [SerializeField] private float cooldownDuration = 5.0f;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private AudioClip shrinkSFX;
    [SerializeField] private AudioClip growSFX;
    [SerializeField] private ParticleSystem shrinkParticles;

    private Vector3 originalScale;
    private bool canShrink = true;
    private bool isShrunk = false;
    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;

    private AudioSource audioSource;
    private Material originalMaterial;

    private void Start()
    {
        originalScale = transform.localScale;
        shrinkParticles.Stop();

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component not found on the child object.");
            return;
        }
        originalMaterials = meshRenderer.materials;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canShrink)
        {
            StartCoroutine(ShrinkCoroutine());
        }

        if (isShrunk)
        {
            AnimateLiquid();
        }
    }

    private IEnumerator ShrinkCoroutine()
    {
        canShrink = false;
        isShrunk = true;

        // Play shrink sound effect
        if (shrinkSFX != null)
        {
            audioSource.PlayOneShot(shrinkSFX);
        }

        // Animate shrinking
        float elapsedTime = 0f;
        while (elapsedTime < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, shrunkScale, elapsedTime / shrinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set final shrunk state
        transform.localScale = shrunkScale;
        //meshRenderer.material = liquidMaterial;
        Material[] shrunkMaterials = new Material[meshRenderer.materials.Length];
        for (int i = 0; i < shrunkMaterials.Length; i++)
        {
            shrunkMaterials[i] = liquidMaterial;
        }
        meshRenderer.materials = shrunkMaterials;
        shrinkParticles.Play();


        yield return new WaitForSeconds(shrunkDuration);

        // Play grow sound effect
        if (growSFX != null)
        {
            audioSource.PlayOneShot(growSFX);
        }

        // Animate growing back
        elapsedTime = 0f;
        while (elapsedTime < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(shrunkScale, originalScale, elapsedTime / shrinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set final original state
        transform.localScale = originalScale;
        meshRenderer.material = originalMaterial;
        shrinkParticles.Stop();

        isShrunk = false;

        // Cooldown
        yield return new WaitForSeconds(cooldownDuration);
        canShrink = true;
    }

    private void AnimateLiquid()
    {
        // Animate the liquid material
        foreach (Material mat in meshRenderer.materials)
        {
            if (mat.HasProperty("_WaveSpeed"))
            {
                float waveSpeed = mat.GetFloat("_WaveSpeed");
                mat.SetFloat("_WaveSpeed", waveSpeed + Time.deltaTime);
            }
        }
    }
}