using System.Collections;
using UnityEngine;
using TMPro;

public class UIMatchMessage : MonoBehaviour
{
    public TMP_Text MessageText;
    public CanvasGroup CanvasGroup;

    [Header("Timing")]
    public float DisplayTime = 2.5f;
    public float FadeSpeed = 3f;

    Coroutine currentRoutine;

    void Awake()
    {
        if (CanvasGroup)
            CanvasGroup.alpha = 0f;
    }

    public void ShowMessage(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    IEnumerator ShowRoutine(string msg)
    {
        MessageText.text = msg;

        // Fade in
        while (CanvasGroup.alpha < 1f)
        {
            CanvasGroup.alpha += Time.deltaTime * FadeSpeed;
            yield return null;
        }

        yield return new WaitForSeconds(DisplayTime);

        // Fade out
        while (CanvasGroup.alpha > 0f)
        {
            CanvasGroup.alpha -= Time.deltaTime * FadeSpeed;
            yield return null;
        }
    }
}