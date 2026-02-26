using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHP = 5.0f;
    public float flatDamage = 2f;
    public float damageScaling = 1.0f;
    public float speed = 2f;
    public float range = 2f;
    public float fireRate = 2f;

    public int coins = 0;
    public int bombs = 0;

    // Flag Modifiers. Subject to change
    public bool piercing = false; // Attacks pass thru enemies while dealing damage
    public bool spectral = false; // Attacks pass thru obstacles
    public bool canFly = false;   // Player ignores obstacles

    public List<OnHitEffect> onHitEffects = new List<OnHitEffect>();

    public float getDamage()
    {
        return flatDamage * damageScaling;
    }
}
