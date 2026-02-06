using UnityEngine;

public class StutteredSparkEmitter : MonoBehaviour
{
    public ParticleSystem sparkSystem;
     public Light sparkLight;
    public float emissionRate = 50f;
    public float activeTime = 0.2f;
    public float pauseTime = 0.1f;

    private float timer = 0f;
    private bool isEmitting = true;

    void Start()
    {
        if (sparkSystem == null)
            sparkSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        sparkLight.intensity = Random.Range(1.2f, 3.0f);

        if (isEmitting && timer >= activeTime)
        {
            StopEmission();
        }
        else if (!isEmitting && timer >= pauseTime)
        {
            StartEmission();
        }
    }

    void StartEmission()
    {
        var emission = sparkSystem.emission;
        emission.rateOverTime = emissionRate;
        isEmitting = true;
        timer = 0f;
    }

    void StopEmission()
    {
        var emission = sparkSystem.emission;
        emission.rateOverTime = 0f;
        isEmitting = false;
        timer = 0f;
    }
}