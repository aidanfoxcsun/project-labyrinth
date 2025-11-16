using UnityEngine;

public class Hazard : MonoBehaviour, IDamager
{
    [SerializeField] private float damage = 2f;
    [SerializeField] private bool isPlayerSourced = false;

    public float DamageAmount => damage;
    public bool PlayerSourced => isPlayerSourced;
}
