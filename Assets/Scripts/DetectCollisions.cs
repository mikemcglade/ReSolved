using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollisions : MonoBehaviour
{
    private GameManager gameManager;
    public int pointValue;
    public ParticleSystem explosionParticle;
    public AudioClip playerHitSound;
    public AudioClip enemyDestroySound;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!gameManager.isPlayerInvincible)
            {
                gameManager.AddLives(-1);
                PlaySound(playerHitSound);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Bullet"))
        {
            gameManager.AddScore(pointValue);
            PlaySound(enemyDestroySound);
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
