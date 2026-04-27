using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyUIController : MonoBehaviour
{
    [Header("Sprite Display (UI Image)")]
    [SerializeField] private Image avatarImage;

    [SerializeField] private TMP_Text level;

    [System.Serializable]
    public class StatUI
    {
        public string label;

        [Header("Progress")]
        public Slider slider;            // progress bar
        public TMP_Text progressText;    // "0/700"

        [Header("Animated Add Text")]
        public TMP_Text animatedAddText; // "+25" pop/count text
        public Vector3 addTextMoveOffset = new Vector3(0, 25f, 0);
    }

    [Header("Stats")]
    public StatUI experience;
    public StatUI coins;
    public StatUI diamonds;

    [Header("Max Values")]
    [SerializeField] private int xpMax = 700;
    [SerializeField] private int coinsMax = 7000;
    [SerializeField] private int diamondsMax = 100; // set whatever you want

    [Header("Animation")]
    [SerializeField] private float countDuration = 0.35f;
    [SerializeField] private float popDuration = 0.6f;

    private int _xp;
    private int _coin;
    private int _diamond;

    private Coroutine _xpAddRoutine;
    private Coroutine _coinsAddRoutine;
    private Coroutine _diamondsAddRoutine;

    void Start()
    {
        // Initialize sliders
        SetupStat(experience, xpMax, _xp);
        SetupStat(coins, coinsMax, _coin);
        SetupStat(diamonds, diamondsMax, _diamond);
    }

    // -------------------------
    // Sprite Display
    // -------------------------
    public void SetAvatarSprite(Sprite sprite)
    {
        if (avatarImage == null) return;
        avatarImage.sprite = sprite;
        avatarImage.enabled = (sprite != null);
    }

    // -------------------------
    // XP / COINS / DIAMONDS API
    // -------------------------
    public void AddExperience(int amount)
    {
        amount = Mathf.Max(0, amount);
        int before = _xp;
        _xp = Mathf.Clamp(_xp + amount, 0, xpMax);

        UpdateStat(experience, xpMax, _xp);

        if (_xpAddRoutine != null) StopCoroutine(_xpAddRoutine);
        _xpAddRoutine = StartCoroutine(PlayAddAnimation(experience, amount));
        StartCoroutine(AnimateSlider(experience.slider, before, _xp, xpMax));
    }

    public void AddCoins(int amount)
    {
        amount = Mathf.Max(0, amount);
        int before = _coin;
        _coin = Mathf.Clamp(_coin + amount, 0, coinsMax);

        UpdateStat(coins, coinsMax, _coin);

        if (_coinsAddRoutine != null) StopCoroutine(_coinsAddRoutine);
        _coinsAddRoutine = StartCoroutine(PlayAddAnimation(coins, amount));
        StartCoroutine(AnimateSlider(coins.slider, before, _coin, coinsMax));
    }

    public void AddDiamonds(int amount)
    {
        amount = Mathf.Max(0, amount);
        int before = _diamond;
        _diamond = Mathf.Clamp(_diamond + amount, 0, diamondsMax);

        UpdateStat(diamonds, diamondsMax, _diamond);

        if (_diamondsAddRoutine != null) StopCoroutine(_diamondsAddRoutine);
        _diamondsAddRoutine = StartCoroutine(PlayAddAnimation(diamonds, amount));
        StartCoroutine(AnimateSlider(diamonds.slider, before, _diamond, diamondsMax));
    }

    // Optional: set from level system directly
    public void SetExperienceFromLevel(int currentXp, int maxXpForLevel)
    {
        xpMax = Mathf.Max(1, maxXpForLevel);
        _xp = Mathf.Clamp(currentXp, 0, xpMax);
        SetupStat(experience, xpMax, _xp);
    }

    // -------------------------
    // Internals
    // -------------------------
    private void SetupStat(StatUI stat, int max, int value)
    {
        if (stat == null) return;

        if (stat.slider != null)
        {
            stat.slider.minValue = 0;
            stat.slider.maxValue = 1;
            stat.slider.value = (max <= 0) ? 0 : (float)value / max;
        }

        if (stat.animatedAddText != null)
        {
            stat.animatedAddText.gameObject.SetActive(false);
        }

        UpdateStat(stat, max, value);
    }

    private void UpdateStat(StatUI stat, int max, int value)
    {
        if (stat == null) return;

        if (stat.progressText != null)
            stat.progressText.text = $"{value}/{max}";

        if (stat.slider != null)
            stat.slider.value = (max <= 0) ? 0 : (float)value / max;
    }

    private IEnumerator AnimateSlider(Slider slider, int fromValue, int toValue, int max)
    {
        if (slider == null || max <= 0) yield break;

        float t = 0f;
        float from = (float)fromValue / max;
        float to = (float)toValue / max;

        while (t < countDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / countDuration);
            slider.value = Mathf.Lerp(from, to, a);
            yield return null;
        }

        slider.value = to;
    }

    private IEnumerator PlayAddAnimation(StatUI stat, int amount)
    {
        if (stat == null || stat.animatedAddText == null) yield break;

        var txt = stat.animatedAddText;
        var rt = txt.GetComponent<RectTransform>();
        if (rt == null) yield break;

        txt.gameObject.SetActive(true);

        // Save base position
        Vector3 basePos = rt.anchoredPosition3D;

        // Count-up text: +0 -> +amount
        float t = 0f;
        while (t < countDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / countDuration);
            int shown = Mathf.RoundToInt(Mathf.Lerp(0, amount, a));
            txt.text = $"+{shown}";
            yield return null;
        }
        txt.text = $"+{amount}";

        // Pop/move upward + fade out
        float popT = 0f;
        Color startColor = txt.color;
        Color c = startColor;

        while (popT < popDuration)
        {
            popT += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(popT / popDuration);

            // Move up
            rt.anchoredPosition3D = Vector3.Lerp(basePos, basePos + stat.addTextMoveOffset, a);

            // Fade out (last half)
            float fade = (a < 0.5f) ? 1f : Mathf.Lerp(1f, 0f, (a - 0.5f) / 0.5f);
            c.a = fade;
            txt.color = c;

            yield return null;
        }

        // Reset
        rt.anchoredPosition3D = basePos;
        c.a = startColor.a;
        txt.color = c;
        txt.gameObject.SetActive(false);
    }
}