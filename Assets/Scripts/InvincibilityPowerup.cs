using UnityEngine;

public class InvincibilityPowerup : MonoBehaviour
{
    public float invincibilityDuration = 7f;
    private GameManager gameManager;
    public AudioClip collectSound;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.ActivateInvincibility(invincibilityDuration);
            PlayCollectSound();
            Destroy(gameObject);
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