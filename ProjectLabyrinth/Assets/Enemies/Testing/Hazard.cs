using UnityEngine;

public class Hazard : MonoBehaviour, IDamager
{
    [SerializeField] private float damage = 2f;
    public float DamageAmount => damage;
}
