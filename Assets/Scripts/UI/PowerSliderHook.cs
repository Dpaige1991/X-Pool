using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PowerSliderHook : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public CueStickController cueStick;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        if (cueStick != null)
            cueStick.SetPower01(value);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cueStick?.OnPowerDragBegin();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // value changes are already handled by onValueChanged
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        cueStick?.OnPowerDragEndAndShoot();

        // Reset slider UI back to 0 after release (optional but feels good)
        slider.SetValueWithoutNotify(0f);
        cueStick?.SetPower01(0f);
    }
}