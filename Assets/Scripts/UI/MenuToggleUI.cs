using UnityEngine;
using UnityEngine.UI;

public class MenuToggleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject openButton;
    [SerializeField] private GameObject closeButton;

    private void Start()
    {
        // Initial State
        menuPanel.SetActive(false);
        openButton.SetActive(true);
        closeButton.SetActive(false);
    }

    public void OpenMenu()
    {
        menuPanel.SetActive(true);
        openButton.SetActive(false);
        closeButton.SetActive(true);
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        closeButton.SetActive(false);
        openButton.SetActive(true);
    }

    public void SettingsMenu()
    {
        settingsPanel.SetActive(true);
    }
}