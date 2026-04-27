using UnityEngine;

public class MenuNavigation_StartMenu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject termsMenu;
    public GameObject privacyMenu;

    private void Start()
    {

    }

    // ---------- Button Actions ----------

    public void OpenTermsMenu()
    {
        mainMenu.SetActive(false);
        termsMenu.SetActive(true);
        privacyMenu.SetActive(false);
    }

    public void OpenPrivacyMenu()
    {
        mainMenu.SetActive(false);
        termsMenu.SetActive(false);
        privacyMenu.SetActive(true);
    }

    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    // ---------- Helper ----------

    private void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        termsMenu.SetActive(false);
        privacyMenu.SetActive(false);
    }
}
