using UnityEngine;

public class ProximitySoundPlayer : MonoBehaviour
{
    public AudioClip soundEffect;
    public float triggerDistance = 5f;

    private AudioSource audioSource;
    private Transform playerTransform;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = soundEffect;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= triggerDistance && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (distanceToPlayer > triggerDistance && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}