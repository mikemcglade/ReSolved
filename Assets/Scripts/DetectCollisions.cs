using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollisions : MonoBehaviour
{
    private GameManager gameManager;
    public int pointValue;
    public ParticleSystem explosionParticle;
    private bool hasCollided = false;
    public AudioClip playerHitSound;
    public AudioClip enemyDestroySound;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision detected with " + other.tag);
        if (other.CompareTag("Player") && !hasCollided)
        {
            Debug.Log("Player collision processed");
            hasCollided = true;
            if (!gameManager.isPlayerInvincible)
            {
                gameManager.AddLives(-1);
                PlaySound(playerHitSound);

                Debug.Log("Player lost a life");
            }
            else
            {
                Debug.Log("Player is invincible, no life lost");
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Bullet"))
        {
            gameManager.AddScore(pointValue);
            PlaySound(enemyDestroySound);
            Destroy(gameObject);
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
            Destroy(other.gameObject);
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
