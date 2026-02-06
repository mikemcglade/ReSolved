using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlintEffect : MonoBehaviour
{
    public float minBrightness = 0.5f;
    public float maxBrightness = 1.5f;
    public float speed = 2f;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, (Mathf.Sin(Time.time * speed) + 1) / 2);
        rend.material.SetColor("_EmissionColor", Color.white * brightness);
    }
}
