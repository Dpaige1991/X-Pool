using UnityEngine;

public class ShowAfterTime : MonoBehaviour
{
    public float delay = 5f;

    void Start()
    {
        // Make sure object starts invisible
        gameObject.SetActive(false);

        // Call method after delay
        Invoke(nameof(ShowObject), delay);
    }

    void ShowObject()
    {
        gameObject.SetActive(true);
    }
}
