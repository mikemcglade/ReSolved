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

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        
        // Set drag to prevent ice skating
        playerRb.drag = 8f;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }
    
    void Update()
    {
        ConstrainPlayerPosition();

        if (Input.GetKeyDown(KeyCode.Space) && Time.time > canFire)
        {
            FireBullet();
        }
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
        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput).normalized;
        
        // Apply movement using MovePosition (no ice skating)
        if (movement.magnitude > 0.1f)
        {
            Vector3 newPosition = playerRb.position + movement * speed * Time.fixedDeltaTime;
            playerRb.MovePosition(newPosition);
            
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            playerRb.rotation = Quaternion.Slerp(playerRb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        
        // Update animator
        this.anim.SetFloat("vertical", verticalInput);
        this.anim.SetFloat("horizontal", horizontalInput);
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
