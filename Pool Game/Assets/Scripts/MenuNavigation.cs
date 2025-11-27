using UnityEngine;
using System.Collections;

public class MenuNavigation : MonoBehaviour
{
    [Header("UI Screens")]
    public GameObject loadingScreen;
    public GameObject loginScreen;

    [Header("Timing")]
    public float loadingDuration = 3f; // seconds before switching

    private void Start()
    {
        // Ensure correct starting state
        loadingScreen.SetActive(true);
        loginScreen.SetActive(false);

        // Start the sequence
        StartCoroutine(ShowLoginAfterDelay());
    }

    private IEnumerator ShowLoginAfterDelay()
    {
        yield return new WaitForSeconds(loadingDuration);
        SwitchToLogin();
    }

    private void SwitchToLogin()
    {
        loadingScreen.SetActive(false);
        loginScreen.SetActive(true);
        Debug.Log("Switched to Login Screen");
    }
}
