using UnityEngine;

public class RainController : MonoBehaviour
{
    private ParticleSystem rainParticleSystem;

    void Start()
    {
        rainParticleSystem = GetComponent<ParticleSystem>();
    }

    public void SetRainIntensity(float intensity)
    {
        var emission = rainParticleSystem.emission;
        emission.rateOverTime = intensity * 100; // Adjust multiplier as needed
    }
}