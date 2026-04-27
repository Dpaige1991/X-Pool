using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PowerSliderUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    public Slider slider;
    public RectTransform powerImage;

    [Header("Settings")]
    public float maxHeight = 300f; // max height of the image

    private bool isHolding = false;

    void Start()
    {
        slider.value = 0;
        powerImage.sizeDelta = new Vector2(powerImage.sizeDelta.x, 0);
        powerImage.gameObject.SetActive(false);

        slider.onValueChanged.AddListener(UpdatePowerVisual);
    }

    void Update()
    {
        // Optional: Auto increase while holding (if you want hold-to-charge)
        if (isHolding)
        {
            slider.value += Time.deltaTime;
            slider.value = Mathf.Clamp01(slider.value);
        }
    }

    void UpdatePowerVisual(float value)
    {
        if (value <= 0)
        {
            powerImage.gameObject.SetActive(false);
            return;
        }

        powerImage.gameObject.SetActive(true);

        float height = value * maxHeight;
        powerImage.sizeDelta = new Vector2(powerImage.sizeDelta.x, height);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;

        // RESET EVERYTHING
        slider.value = 0;
        powerImage.sizeDelta = new Vector2(powerImage.sizeDelta.x, 0);
        powerImage.gameObject.SetActive(false);
    }
}