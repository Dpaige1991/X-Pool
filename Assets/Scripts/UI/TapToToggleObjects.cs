using UnityEngine;

public class TapToToggleObjects : MonoBehaviour
{
    [SerializeField] private GameObject triggerObject; // The object that must be visible for the tap to work
    [SerializeField] private GameObject objectToHide1; // First object to hide
    [SerializeField] private GameObject objectToHide2; // Second object to hide
    [SerializeField] private GameObject objectToShow;

    private void Update()
    {
        // Check for tap or mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Only respond if the trigger object is active in the scene
            if (triggerObject != null && triggerObject.activeInHierarchy)
            {
                HideObjects();
            }
        }
    }

    private void HideObjects()
    {
        if (objectToHide1 != null)
            objectToHide1.SetActive(false);

        if (objectToHide2 != null)
            objectToHide2.SetActive(false);

        if (objectToShow != null)
            objectToShow.SetActive(true);
    }
}
