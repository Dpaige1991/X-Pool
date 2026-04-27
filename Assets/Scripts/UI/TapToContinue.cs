using UnityEngine;

public class TapToContinue : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tapToContinueUI;
    public GameObject signInMenuUI;
    public GameObject termsAndPrivacyUI;

    private void Start()
    {
        // Make sure starting state is correct
        tapToContinueUI.SetActive(true);
        signInMenuUI.SetActive(false);
        termsAndPrivacyUI.SetActive(true);
    }

    // Call this from the button OnClick
    public void OnTapToContinue()
    {
        Debug.Log("Pressed");
        tapToContinueUI.SetActive(false);
        signInMenuUI.SetActive(true);
        termsAndPrivacyUI.SetActive(false);
    }
}
