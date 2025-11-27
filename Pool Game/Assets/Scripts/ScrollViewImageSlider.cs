using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScrollViewImageSlider : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect;        // The Scroll View component
    public RectTransform content;        // The Content inside Scroll View
    public Button nextButton;            // Button to slide right
    public Button prevButton;            // Button to slide left

    [Header("Slide Settings")]
    public float slideSpeed = 10f;       // How fast the slide animates
    public int totalImages = 3;          // Number of child images inside Content

    private int currentIndex = 0;
    private float targetPosX = 0f;

    void Start()
    {
        // Button setup
        if (nextButton != null)
            nextButton.onClick.AddListener(NextImage);

        if (prevButton != null)
            prevButton.onClick.AddListener(PrevImage);

        // Ensure ScrollRect starts at the first image
        SetScrollPosition(0);
    }

    void Update()
    {
        // Smoothly move to target position
        Vector2 currentPos = scrollRect.normalizedPosition;
        float newX = Mathf.Lerp(currentPos.x, targetPosX, Time.deltaTime * slideSpeed);
        scrollRect.normalizedPosition = new Vector2(newX, currentPos.y);
    }

    void NextImage()
    {
        if (currentIndex < totalImages - 1)
            currentIndex++;
        else
            currentIndex = 0; // loop around

        UpdateTargetPosition();
    }

    void PrevImage()
    {
        if (currentIndex > 0)
            currentIndex--;
        else
            currentIndex = totalImages - 1; // loop around

        UpdateTargetPosition();
    }

    void UpdateTargetPosition()
    {
        // Evenly space each image based on total count
        if (totalImages > 1)
            targetPosX = (float)currentIndex / (totalImages - 1);
        else
            targetPosX = 0f;
    }

    void SetScrollPosition(int index)
    {
        currentIndex = index;
        UpdateTargetPosition();
        scrollRect.normalizedPosition = new Vector2(targetPosX, 0);
    }
}
