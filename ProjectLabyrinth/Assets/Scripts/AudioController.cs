using System.Collections;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [SerializeField] AudioSource mainMenuSource;
    [SerializeField] AudioSource ambientSource_normal;
    [SerializeField] AudioSource ambientSource_shop;
    [SerializeField] AudioSource ambientSource_item;
    [SerializeField] AudioSource bossSource;

    [Range(0f, 1f)] public float masterVolume = 1.0f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;
    [SerializeField] float crossfadeDuration = 2.0f;

    // Target volumes for each layer (0-1, before master volume scaling)
    private float target_mainMenu = 1f; // Start with main menu music at full volume
    private float target_normal = 0f;
    private float target_shop = 0f;
    private float target_item = 0f;
    private float target_boss = 0f;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Silence all game layers at start
        ambientSource_normal.volume = 0f;
        ambientSource_shop.volume = 0f;
        ambientSource_item.volume = 0f;
        bossSource.volume = 0f;

        mainMenuSource.volume = masterVolume;
        target_mainMenu = 1f;
        mainMenuSource.Play();
    }

    public void ReturnToMenu()
    {
        SetTargets(normal: 0f, shop: 0f, item: 0f, boss: 0f);
        mainMenuSource.Play();
    }

    // -------------------------------------------------------
    // Public transition calls
    // -------------------------------------------------------

    /// <summary>Call when leaving the main menu and starting gameplay.</summary>
    public void StartGame()
    {
        mainMenuSource.Stop();
        target_mainMenu = 1f;

        // Start all sources simultaneously so they stay in sync
        ambientSource_normal.Play();
        ambientSource_shop.Play();
        ambientSource_item.Play();
        bossSource.Play();

        // Bring normal ambience up immediately
        SetTargets(normal: 1f, shop: 0f, item: 0f, boss: 0f);
    }

    public void EnterNormalArea() => SetTargets(normal: 1f, shop: 0f, item: 0f, boss: 0f);
    public void EnterShop() => SetTargets(normal: 0f, shop: 1f, item: 0f, boss: 0f);
    public void EnterItemRoom() => SetTargets(normal: 0f, shop: 0f, item: 1f, boss: 0f);
    public void EnterBossRoom() => SetTargets(normal: 0f, shop: 0f, item: 0f, boss: 1f);

    public void SetVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        Debug.Log($"Master volume set to {masterVolume * 100}%");
        // Current volumes will be corrected next Update tick via lerp
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        Debug.Log($"SFX volume set to {sfxVolume * 100}%");
        // This will affect any sound effects played after this call, but won't retroactively change currently playing ones
    }

    // -------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------

    private void SetTargets(float normal, float shop, float item, float boss)
    {
        target_normal = normal;
        target_shop = shop;
        target_item = item;
        target_boss = boss;
    }

    void Update()
    {

        float step = Time.deltaTime / crossfadeDuration;

        mainMenuSource.volume = Mathf.MoveTowards(mainMenuSource.volume, target_mainMenu * masterVolume, step);
        ambientSource_normal.volume = Mathf.MoveTowards(ambientSource_normal.volume, target_normal * masterVolume, step);
        ambientSource_shop.volume = Mathf.MoveTowards(ambientSource_shop.volume, target_shop * masterVolume, step);
        ambientSource_item.volume = Mathf.MoveTowards(ambientSource_item.volume, target_item * masterVolume, step);
        bossSource.volume = Mathf.MoveTowards(bossSource.volume, target_boss * masterVolume, step);
    }
}