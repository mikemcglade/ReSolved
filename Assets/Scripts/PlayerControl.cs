using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5.0f;
    public float rotationSpeed = 10f;
    
    [Header("Combat Settings")]
    private float zBound = 10;
    public float fireRate = 1f;
    public float canFire = -1f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public bool hasPowerup;
    
    private Rigidbody playerRb;
    public Animator anim;
    private PlayerShrink playerShrink;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        
        // Set drag to prevent ice skating
        playerRb.drag = 8f;
        
        // Get PlayerShrink component
        playerShrink = GetComponent<PlayerShrink>();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }
    
    void Update()
    {
        ConstrainPlayerPosition();

        // Can't fire while shrunk
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > canFire && !IsShrunk())
        {
            FireBullet();
        }
    }
    
    private bool IsShrunk()
    {
        return playerShrink != null && playerShrink.IsShrunk;
    }
    
    private void FireBullet()
    {
        canFire = Time.time + fireRate;
        // Spawn projectile facing the same direction as the player
        Instantiate(projectilePrefab, projectileSpawnPoint.position, transform.rotation);
    }

    void MovePlayer()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Create movement direction
        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);
        
        // Apply movement using MovePosition (no ice skating)
        if (movement.magnitude > 0.1f)
        {
            // Normalize the movement
            movement = movement.normalized;
            
            // Snap to 8 directions (N, NE, E, SE, S, SW, W, NW)
            Vector3 snappedDirection = SnapToEightDirections(movement);
            
            Vector3 newPosition = playerRb.position + snappedDirection * speed * Time.fixedDeltaTime;
            playerRb.MovePosition(newPosition);
            
            // Rotate player to face snapped direction (faster smoothing)
            Quaternion targetRotation = Quaternion.LookRotation(snappedDirection);
            playerRb.rotation = Quaternion.Slerp(playerRb.rotation, targetRotation, 20f * Time.fixedDeltaTime);
        }
        
        // Update animator
        this.anim.SetFloat("vertical", verticalInput);
        this.anim.SetFloat("horizontal", horizontalInput);
    }
    
    Vector3 SnapToEightDirections(Vector3 direction)
    {
        // Calculate angle from direction
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        // Snap to nearest 45-degree increment
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;
        
        // Convert back to direction vector
        float radians = snappedAngle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
    }

    void ConstrainPlayerPosition()
    {
        if (transform.position.z < -zBound)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -zBound);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            Destroy(other.gameObject);
            StartCoroutine(PowerupCountdownRoutine());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Enemy") && hasPowerup)
        {
            Destroy(gameObject);
            Debug.Log("Collided with " + collision.gameObject.name + " with powerup set to " + hasPowerup);
        }
    }

    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;
    }
}
