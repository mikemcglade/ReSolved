using UnityEngine;

public class PulsingSpotLight : MonoBehaviour
{
    public float minIntensity = 15f;
    public float maxIntensity = 30f;
    public float pulseSpeed = 0.5f;

    private Light spotLight;

    void Start()
    {
        spotLight = GetComponent<Light>();
    }

    void Update()
    {
        float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        spotLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
    }
}