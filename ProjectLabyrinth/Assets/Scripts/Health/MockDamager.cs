using UnityEngine;

public class MockDamager : MonoBehaviour, IDamager
{
    public float DamageAmount => 10f;
    public bool PlayerSourced => false;
}