using UnityEngine;

public class PortalAnimator : MonoBehaviour
{
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.1f;

    private ParticleSystem portalParticles;
    private Material portalMaterial;

    void Start()
    {
        portalParticles = GetComponent<ParticleSystem>();
        portalMaterial = portalParticles.GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Animate particle emission rate
        var emission = portalParticles.emission;
        emission.rateOverTime = 50 + Mathf.Sin(Time.time * pulseSpeed) * 20;

        // Animate material properties
        float pulseFactor = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        portalMaterial.SetFloat("_DistortionStrength", 0.1f + pulseFactor);
        portalMaterial.SetFloat("_ColorVariation", 0.1f + pulseFactor);
    }
}