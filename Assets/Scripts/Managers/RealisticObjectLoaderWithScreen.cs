using UnityEngine;
using System.Collections;

public class RealisticObjectLoaderWithScreen : MonoBehaviour
{
    [Header("Objects")]
    public GameObject objectToShow;     // The object to appear after loading
    public GameObject loadingScreen;    // The loading screen to hide after loading

    [Header("Settings")]
    public float baseLoadTime = 3f;     // Base loading time for an average device

    private void Start()
    {
        // Hide object initially
        if (objectToShow != null)
            objectToShow.SetActive(false);

        // Show loading screen if assigned
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        StartCoroutine(LoadObjectBasedOnDevice());
    }

    public void ShowLoadingScreen()
    {
        StartCoroutine(LoadObjectBasedOnDevice());
    }

    IEnumerator LoadObjectBasedOnDevice()
    {
        // Measure device performance
        int framesToTest = 60;
        float totalFrameTime = 0f;

        for (int i = 0; i < framesToTest; i++)
        {
            float start = Time.realtimeSinceStartup;
            yield return null; // Wait one frame
            totalFrameTime += (Time.realtimeSinceStartup - start);
        }

        float averageFrameTime = totalFrameTime / framesToTest;
        float averageFPS = 1f / averageFrameTime;

        // Determine load time multiplier
        float performanceMultiplier = Mathf.Clamp(60f / averageFPS, 0.6f, 2.0f);

        // Wait simulated loading time
        float simulatedLoadTime = baseLoadTime * performanceMultiplier;
        yield return new WaitForSeconds(simulatedLoadTime);

        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // Show the object
        if (objectToShow != null)
            objectToShow.SetActive(true);
    }
}
