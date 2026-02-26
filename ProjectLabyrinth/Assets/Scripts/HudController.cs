using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    // =========================================================
    // Hearts (0.5 step system)
    // =========================================================
    [Header("Hearts (0.5 per hit)")]
    [Tooltip("Left-to-right order. Image Type = Filled (Horizontal Left).")]
    public Image[] heartImages;

    [Min(1)] public int maxHearts = 5;
    [SerializeField] private float currentHearts;

    // =========================================================
    // Timer
    // =========================================================
    [Header("Timer")]
    public TMP_Text timerText;
    public bool countDown = false;
    public int startTimeSeconds = 0;

    private float timeCounter;
    private bool timerRunning = true;

    // =========================================================
    // Boss Health Bar
    // =========================================================
    [Header("Boss Health")]
    public GameObject bossUIRoot;
    public Slider bossHealthSlider;
    public TMP_Text bossNameText;

    private float bossMaxHP = 0f;

    // =========================================================
    // Item Collection (Vertical Layout)
    // =========================================================
    [Header("Item Collection")]
    public Transform itemIconContainer;
    public GameObject itemIconPrefab;

    [Tooltip("Pop animation duration")]
    public float itemPopDuration = 0.18f;

    [Tooltip("Pop overshoot scale (1.2 = 20% larger)")]
    public float itemPopOvershoot = 1.2f;

    private Dictionary<string, ItemSlotUI> itemSlots = new Dictionary<string, ItemSlotUI>();

    private class ItemSlotUI
    {
        public RectTransform rect;
        public Image icon;
        public TMP_Text countText;
        public int count;
        public Coroutine anim;
    }

    // =========================================================
    // Bomb Counter
    // =========================================================
    [Header("Bomb Counter")]
    public Image bombIcon;
    public TMP_Text bombCountText;
    private int bombCount = 0;

    // =========================================================
    // Coin Counter
    // =========================================================
    [Header("Coin Counter")]
    public Image coinIcon;
    public TMP_Text coinCountText;
    private int coinCount = 0;

    // =========================================================
    // Initialization
    // =========================================================
    void Awake()
    {
        // Hearts
        if (currentHearts <= 0f) currentHearts = maxHearts;
        currentHearts = ClampToHalfStep(currentHearts, 0f, maxHearts);
        UpdateHeartsUI();

        // Timer
        timeCounter = countDown ? Mathf.Max(0, startTimeSeconds) : 0f;
        UpdateTimerUI();

        // Boss hidden initially
        if (bossUIRoot != null)
            bossUIRoot.SetActive(false);

        // Counters
        UpdateBombUI();
        UpdateCoinUI();
    }

    void Update()
    {
        if (!timerRunning) return;

        timeCounter += (countDown ? -1f : 1f) * Time.deltaTime;

        if (countDown && timeCounter <= 0f)
        {
            timeCounter = 0f;
            timerRunning = false;
        }

        UpdateTimerUI();
    }

    // =========================================================
    // Hearts API
    // =========================================================
    public void DamageHalfHeart() => AddHearts(-0.5f);
    public void HealHalfHeart() => AddHearts(0.5f);

    public void AddHearts(float delta)
    {
        SetHearts(currentHearts + delta);
    }

    public void SetHearts(float hearts)
    {
        currentHearts = ClampToHalfStep(hearts, 0f, maxHearts);
        UpdateHeartsUI();
    }

    private void UpdateHeartsUI()
    {
        if (heartImages == null) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            float remaining = currentHearts - i;

            float fill =
                remaining >= 1f ? 1f :
                remaining >= 0.5f ? 0.5f : 0f;

            if (heartImages[i] != null)
                heartImages[i].fillAmount = fill;
        }
    }

    private static float ClampToHalfStep(float v, float min, float max)
    {
        v = Mathf.Clamp(v, min, max);
        return Mathf.Round(v * 2f) / 2f;
    }

    // =========================================================
    // Timer API
    // =========================================================
    public void StartTimer() => timerRunning = true;
    public void PauseTimer() => timerRunning = false;

    public void ResetTimer()
    {
        timeCounter = countDown ? Mathf.Max(0, startTimeSeconds) : 0f;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int total = Mathf.FloorToInt(timeCounter);
        int m = total / 60;
        int s = total % 60;

        timerText.text = $"{m:00}:{s:00}";
    }

    // =========================================================
    // Boss Health API
    // =========================================================
    public void ShowBoss(string bossName, float maxHP)
    {
        bossMaxHP = Mathf.Max(1f, maxHP);

        if (bossUIRoot != null)
            bossUIRoot.SetActive(true);

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (bossHealthSlider != null)
        {
            bossHealthSlider.minValue = 0;
            bossHealthSlider.maxValue = bossMaxHP;
            bossHealthSlider.value = bossMaxHP;
        }
    }

    public void SetBossHP(float currentHP)
    {
        if (bossHealthSlider == null) return;

        bossHealthSlider.value = Mathf.Clamp(currentHP, 0, bossMaxHP);
    }

    public void HideBoss()
    {
        if (bossUIRoot != null)
            bossUIRoot.SetActive(false);
    }

    // =========================================================
    // Item Collection (Vertical Pop-In)
    // =========================================================
    public void AddCollectedItem(string key, Sprite iconSprite, int amount = 1)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (itemIconContainer == null || itemIconPrefab == null) return;

        if (!itemSlots.TryGetValue(key, out ItemSlotUI slot))
        {
            GameObject go = Instantiate(itemIconPrefab, itemIconContainer);
            RectTransform rt = go.GetComponent<RectTransform>();

            Image img = go.GetComponentInChildren<Image>();
            TMP_Text countTmp = go.GetComponentInChildren<TMP_Text>();

            if (img != null) img.sprite = iconSprite;

            slot = new ItemSlotUI
            {
                rect = rt,
                icon = img,
                countText = countTmp,
                count = 0
            };

            rt.localScale = Vector3.zero;
            itemSlots[key] = slot;
        }

        slot.count += amount;

        if (slot.countText != null)
            slot.countText.text = slot.count.ToString();

        if (slot.rect != null)
        {
            if (slot.anim != null) StopCoroutine(slot.anim);
            slot.anim = StartCoroutine(PopIn(slot.rect));
        }
    }

    private IEnumerator PopIn(RectTransform rt)
    {
        float duration = itemPopDuration;
        float overshoot = itemPopOvershoot;

        float half = duration * 0.6f;
        float t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float a = t / half;
            float s = Mathf.Lerp(0f, overshoot, a);
            rt.localScale = Vector3.one * s;
            yield return null;
        }

        float t2 = 0f;
        float half2 = duration - half;

        while (t2 < half2)
        {
            t2 += Time.unscaledDeltaTime;
            float a = t2 / half2;
            float s = Mathf.Lerp(overshoot, 1f, a);
            rt.localScale = Vector3.one * s;
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    // =========================================================
    // Bomb API
    // =========================================================
    public void SetBombs(int amount)
    {
        bombCount = Mathf.Max(0, amount);
        UpdateBombUI();
    }

    public void AddBombs(int amount)
    {
        bombCount = Mathf.Max(0, bombCount + amount);
        UpdateBombUI();

        if (bombIcon != null)
            StartCoroutine(PopIn(bombIcon.rectTransform));
    }

    public bool UseBomb(int amount = 1)
    {
        if (bombCount < amount) return false;

        bombCount -= amount;
        UpdateBombUI();
        return true;
    }

    private void UpdateBombUI()
    {
        if (bombCountText != null)
            bombCountText.text = bombCount.ToString();

        if (bombIcon != null)
            bombIcon.enabled = true;
    }

    // =========================================================
    // Coin API
    // =========================================================
    public void SetCoins(int amount)
    {
        coinCount = Mathf.Max(0, amount);
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        coinCount = Mathf.Max(0, coinCount + amount);
        UpdateCoinUI();

        if (coinIcon != null)
            StartCoroutine(PopIn(coinIcon.rectTransform));
    }

    public bool SpendCoins(int amount)
    {
        if (coinCount < amount) return false;

        coinCount -= amount;
        UpdateCoinUI();
        return true;
    }

    private void UpdateCoinUI()
    {
        if (coinCountText != null)
            coinCountText.text = coinCount.ToString();

        if (coinIcon != null)
            coinIcon.enabled = true;
    }
}