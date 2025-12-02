using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 14f;
    public float lifetime = 5f;
    Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        // Moves forward every frame
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Room")) return; // don't self-hit
        Destroy(gameObject, 0.05f);
    }
}