using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject CueMenuPanel;
    public GameObject SettingsPanel;

    public void ToggleSettings()
    {
        SettingsPanel.SetActive(!SettingsPanel.activeSelf);
    }

    public void ToggleCueMenu()
    {
        CueMenuPanel.SetActive(!CueMenuPanel.activeSelf);
    }
}
