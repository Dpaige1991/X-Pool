using UnityEngine;

public class ToggleOnClick : MonoBehaviour
{
    [SerializeField] private GameObject newObject; // Assign in Inspector

    void Update()
    {
        // Only check input if this GameObject is active
        if (gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            // Deactivate the current GameObject
            gameObject.SetActive(false);

            // Activate the new GameObject
            if (newObject != null)
                newObject.SetActive(true);
        }
    }
}
