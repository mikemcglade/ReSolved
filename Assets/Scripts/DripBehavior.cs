using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DripBehavior : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.velocity = Vector3.zero; // Ensure the drip starts with no initial velocity
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.AddLives(-1);
            Destroy(gameObject);
        }
    }
}