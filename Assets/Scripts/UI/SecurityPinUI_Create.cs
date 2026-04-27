using UnityEngine;
using TMPro;

public class SecurityPinUI_Create : MonoBehaviour
{
    [Header("Menus")]
    public GameObject signInMenu;
    public GameObject createPinMenu;
    public GameObject confirmPinMenu;
    public GameObject avatarMenu;

    [Header("Input Fields")]
    public TMP_InputField createPinInput;
    public TMP_InputField confirmPinInput;

    private string tempPin;

    // ---------- CREATE PIN ----------
    public void OnCreatePinConfirm()
    {
        if (string.IsNullOrEmpty(createPinInput.text))
            return;

        tempPin = createPinInput.text;
        createPinInput.text = "";

        createPinMenu.SetActive(false);
        confirmPinMenu.SetActive(true);
    }

    // ---------- CONFIRM PIN ----------
    public void OnConfirmPinConfirm()
    {
        if (confirmPinInput.text != tempPin)
        {
            confirmPinInput.text = "";
            return;
        }

        PlayerDataManager.Instance.CurrentPlayerData.securityPin = tempPin;
        PlayerDataManager.Instance.SaveData();

        tempPin = "";
        confirmPinInput.text = "";

        ShowAvatarMenu();
    }

    // ---------- CANCEL ----------
    public void OnCancelPin()
    {
        tempPin = "";
        createPinInput.text = "";
        confirmPinInput.text = "";

        createPinMenu.SetActive(false);
        confirmPinMenu.SetActive(false);
        signInMenu.SetActive(true);
    }

    // ---------- UI STATES ----------
    void ShowCreatePin()
    {
        signInMenu.SetActive(false);
        createPinMenu.SetActive(true);
        confirmPinMenu.SetActive(false);
        avatarMenu.SetActive(false);
    }

    void ShowAvatarMenu()
    {
        signInMenu.SetActive(false);
        createPinMenu.SetActive(false);
        confirmPinMenu.SetActive(false);
        avatarMenu.SetActive(true);
    }
}
