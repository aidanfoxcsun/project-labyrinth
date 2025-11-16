using UnityEngine;

public class Sword : MonoBehaviour
{
    public int damage = 1;
    public GameObject owner;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        if (other.CompareTag("Enemy"))
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
    }
}
