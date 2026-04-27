using UnityEngine;
using System.Collections;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Canvas References")]
    public Canvas canvasA;
    public Canvas canvasB;

    [Header("Switch Delay")]
    public float switchTime = 5f;

    void Start()
    {
        // Ensure initial state
        canvasA.gameObject.SetActive(true);
        canvasB.gameObject.SetActive(false);

        StartCoroutine(SwitchCanvasAfterDelay());
    }

    IEnumerator SwitchCanvasAfterDelay()
    {
        yield return new WaitForSeconds(switchTime);

        canvasA.gameObject.SetActive(false);
        canvasB.gameObject.SetActive(true);
    }
}
