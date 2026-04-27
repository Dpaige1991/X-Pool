using UnityEngine;
using UnityEngine.UI;

public class ScrollContentNavigator : MonoBehaviour
{
    [Header("Scroll References")]
    public ScrollRect scrollRect;
    public RectTransform content;

    [Header("Buttons")]
    public GameObject buttonGoRight; // Button that moves to -2980
    public GameObject buttonGoLeft;  // Button that moves back to 0
    public GameObject lekkiOutline;
    public GameObject beninOutline;

    [Header("Buttons")]
    public Button buttonLekki;
    public Button buttonBenin;

    [Header("Positions")]
    public float leftX = 0f;
    public float rightX = -2980f;

    void Awake()
    {
        if (content == null)
            content = scrollRect.content;

        // Initial state
        buttonBenin.interactable = false;
        buttonLekki.interactable = true;
    }

    // ➡️ Move to the right
    public void MoveRight()
    {
        SetContentX(rightX);
        buttonGoRight.SetActive(false);
        buttonGoLeft.SetActive(true);
        lekkiOutline.SetActive(false);
        beninOutline.SetActive(true);

        buttonBenin.interactable = true;
        buttonLekki.interactable = false;
    }

    // ⬅️ Move to the left
    public void MoveLeft()
    {
        SetContentX(leftX);
        buttonGoLeft.SetActive(false);
        buttonGoRight.SetActive(true);
        lekkiOutline.SetActive(true);
        beninOutline.SetActive(false);

        buttonBenin.interactable = false;
        buttonLekki.interactable = true;
    }

    void SetContentX(float x)
    {
        Vector2 pos = content.anchoredPosition;
        pos.x = x;
        content.anchoredPosition = pos;
    }
}
