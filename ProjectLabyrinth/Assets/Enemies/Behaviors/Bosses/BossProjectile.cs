using System.Collections;
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public Vector3 direction;

    public float lifetime = 2f;
    public float speed = 5f;

    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position = transform.position + direction * speed * Time.deltaTime;

        if(timer > lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Hit Collider!");
        StartCoroutine(OffsetDestroy());
    }

    private IEnumerator OffsetDestroy()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}
