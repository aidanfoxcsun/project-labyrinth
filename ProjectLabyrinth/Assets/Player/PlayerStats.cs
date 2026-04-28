using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // =========================================================
    // HUD Reference
    // =========================================================
    private HUDController hud;

    void Awake()
    {
        hud = FindFirstObjectByType<HUDController>();
    }

    void Start()
    {
        // Push all initial values to HUD on spawn
        SyncAll();
    }

    // =========================================================
    // HP
    // =========================================================
    [SerializeField] private float _maxHP = 5f;
    [SerializeField] private float _currentHP = 5f;

    public float maxHP
    {
        get => _maxHP;
        set { _maxHP = Mathf.Max(1f, value); SyncHearts(); }
    }

    public float currentHP
    {
        get => _currentHP;
        set
        {
            _currentHP = Mathf.Clamp(value, 0f, _maxHP);
            hud?.SetHearts(_currentHP);
            if (_currentHP <= 0f) OnDeath();
        }
    }

    public void TakeDamage(float amount) => currentHP -= amount;
    public void Heal(float amount) => currentHP += amount;

    // =========================================================
    // Stats (no HUD binding needed — call SyncStats if you
    // want a stat readout panel later)
    // =========================================================
    public float flatDamage = 2f;
    public float damageScaling = 1.0f;
    public float speed = 2f;
    public float range = 2f;
    public float fireRate = 2f;

    public float getDamage() => flatDamage * damageScaling;

    // =========================================================
    // Coins
    // =========================================================
    [SerializeField] private int _coins = 0;

    public int coins
    {
        get => _coins;
        set
        {
            int delta = value - _coins;
            _coins = Mathf.Max(0, value);

            if (delta > 0) hud?.AddCoins(delta);
            else hud?.SetCoins(_coins);
        }
    }

    public bool SpendCoins(int amount)
    {
        if (_coins < amount) return false;
        coins -= amount;
        return true;
    }

    // =========================================================
    // Bombs
    // =========================================================
    [SerializeField] private int _bombs = 0;

    public int bombs
    {
        get => _bombs;
        set
        {
            int delta = value - _bombs;
            _bombs = Mathf.Max(0, value);

            if (delta > 0) hud?.AddBombs(delta);
            else hud?.SetBombs(_bombs);
        }
    }

    public bool UseBomb()
    {
        if (_bombs <= 0) return false;
        bombs -= 1;
        return true;
    }

    // =========================================================
    // Flags
    // =========================================================
    public bool piercing = false;
    public bool spectral = false;
    public bool canFly = false;

    // =========================================================
    // On Hit Effects
    // =========================================================
    public List<OnHitEffect> onHitEffects = new List<OnHitEffect>();

    // =========================================================
    // Item Collection
    // =========================================================
    public void CollectItem(string key, Sprite icon, int amount = 1)
    {
        hud?.AddCollectedItem(key, icon, amount);
    }

    // =========================================================
    // Sync Helpers
    // =========================================================
    private void SyncHearts()
    {
        // Clamp currentHP if maxHP shrunk (e.g. Dead Cat)
        _currentHP = Mathf.Clamp(_currentHP, 0f, _maxHP);
        hud?.SetHearts(_currentHP);
    }

    private void SyncAll()
    {
        if (hud == null) return;
        hud.SetHearts(_currentHP);
        hud.SetCoins(_coins);
        hud.SetBombs(_bombs);
    }

    private void OnDeath()
    {
        Debug.Log("[PlayerStats] Player died.");
        // Hook into your game manager / respawn system here
    }
}