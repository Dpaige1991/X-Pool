using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SecurityPinEntryUI : MonoBehaviour
{
    [Header("Menus")]
    public GameObject signInMenu;
    public GameObject enterPinMenu;
    public GameObject avatarMenu;

    [Header("Input")]
    public TMP_InputField enterPinInput;

    [Header("Scene Transition")]
    public string nextSceneName;

    // ---------- CONFIRM PIN ----------
    public void OnConfirmPin()
    {
        if (enterPinInput.text == PlayerDataManager.Instance.CurrentPlayerData.securityPin)
        {
            if(PlayerDataManager.Instance.CurrentPlayerData.avatarSprite == null)
            {
                enterPinInput.text = "";
                ShowMainMenu();
            }
            else
            {
                enterPinInput.text = "";
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            enterPinInput.text = "";
            Debug.Log("Incorrect PIN");
        }
    }

    // ---------- CANCEL ----------
    public void OnCancelPin()
    {
        enterPinInput.text = "";

        enterPinMenu.SetActive(false);
        avatarMenu.SetActive(false);
        signInMenu.SetActive(true);
    }

    // ---------- UI STATES ----------
    void ShowEnterPin()
    {
        signInMenu.SetActive(false);
        enterPinMenu.SetActive(true);
        avatarMenu.SetActive(false);
    }

    void ShowMainMenu()
    {
        signInMenu.SetActive(false);
        enterPinMenu.SetActive(false);
        avatarMenu.SetActive(true);
    }
}
