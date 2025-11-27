using UnityEngine;
using System.Collections;

public class ShowObjectAfterDelay : MonoBehaviour
{
    [SerializeField] private GameObject objectToShow; // The object to enable
    [SerializeField] private float delay = 3f; // Seconds to wait before showing

    private void Start()
    {
        // Start the coroutine that handles the delay
        StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        // Hide the object at start (optional)
        if (objectToShow != null)
            objectToShow.SetActive(false);

        // Wait for the set amount of seconds
        yield return new WaitForSeconds(delay);

        // Show the object
        if (objectToShow != null)
            objectToShow.SetActive(true);
    }
}

