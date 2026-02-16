using System.Collections;
using UnityEngine;

public class FlamethrowerWeapon : MonoBehaviour
{
    [Header("Flamethrower Settings")]
    public float range = 3f;
    public float coneAngle = 60f;
    public float duration = 0.5f;
    public float cooldown = 2f;
    
    [Header("Visual Effects")]
    public ParticleSystem flameEffect;
    
    [Header("Audio")]
    public AudioClip flamethrowerSound;
    public AudioClip enemyKillSound;
    
    private bool canFire = true;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;
    private PlayerShrink playerShrink;

    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get PlayerShrink component
        playerShrink = GetComponent<PlayerShrink>();
    }

    void Update()
    {
        // Update cooldown timer
        if (!canFire)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canFire = true;
            }
        }

        // Fire flamethrower on E key ONLY if not near an interactable object AND not shrunk
        bool isShrunk = playerShrink != null && playerShrink.IsShrunk;
        if (Input.GetKeyDown(KeyCode.E) && canFire && !InteractableObject.PlayerNearInteractable && !isShrunk)
        {
            FireFlamethrower();
        }
    }

    void FireFlamethrower()
    {
        canFire = false;
        cooldownTimer = cooldown;
        
        // Play flamethrower sound
        if (flamethrowerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(flamethrowerSound);
        }
        
        // Start visual effect
        if (flameEffect != null)
        {
            flameEffect.Play();
        }
        
        // Detect and damage enemies in cone
        StartCoroutine(FlamethrowerDamage());
    }

    IEnumerator FlamethrowerDamage()
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            DamageEnemiesInCone();
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Stop visual effect
        if (flameEffect != null)
        {
            flameEffect.Stop();
        }
    }

    void DamageEnemiesInCone()
    {
        // Find all colliders in range (no layer filter)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        
        foreach (Collider col in hitColliders)
        {
            // Only damage objects with "Enemy" tag
            if (col.CompareTag("Enemy"))
            {
                // Check if enemy is within cone angle
                Vector3 directionToEnemy = (col.transform.position - transform.position).normalized;
                float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy);
                
                if (angleToEnemy < coneAngle / 2f)
                {
                    // Get enemy's point value and add to score
                    DetectCollisions detectCollisions = col.GetComponent<DetectCollisions>();
                    if (detectCollisions != null)
                    {
                        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                        if (gameManager != null)
                        {
                            gameManager.AddScore(detectCollisions.pointValue);
                        }
                        
                        // Spawn explosion particle effect
                        if (detectCollisions.explosionParticle != null)
                        {
                            Instantiate(detectCollisions.explosionParticle, col.transform.position, detectCollisions.explosionParticle.transform.rotation);
                        }
                    }
                    
                    // Play kill sound with slight delay
                    if (enemyKillSound != null)
                    {
                        StartCoroutine(PlayDelayedSound(enemyKillSound, col.transform.position, 0.1f));
                    }
                    
                    // Enemy is in cone - destroy it
                    Destroy(col.gameObject);
                }
            }
        }
    }

    // Visualize cone in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        // Draw cone visualization
        Vector3 forward = transform.forward * range;
        Vector3 right = Quaternion.Euler(0, coneAngle / 2f, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -coneAngle / 2f, 0) * forward;
        
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + right);
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position + right, transform.position + left);
    }
    
    IEnumerator PlayDelayedSound(AudioClip clip, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioSource.PlayClipAtPoint(clip, position);
    }
}

