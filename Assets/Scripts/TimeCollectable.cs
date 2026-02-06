using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCollectable : MonoBehaviour
{
    public float timeToAdd = 20f;
    public AudioClip collectSound;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CountdownTimer timer = FindObjectOfType<CountdownTimer>();
            if (timer != null)
            {
                timer.AddTime(timeToAdd);
                PlayCollectSound();
                Destroy(gameObject);
            }
        }
    }

    private void PlayCollectSound()
    {
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}