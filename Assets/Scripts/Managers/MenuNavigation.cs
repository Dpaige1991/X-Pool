using UnityEngine;

public class MenuNavigation : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject poolPassPanel;
    public GameObject chatPanel;
    public GameObject friendsPanel;
    public GameObject settingsPanel;
    public GameObject eventsPanel;
    public GameObject play1v1Panel;
    public GameObject playWithFriendsPanel;
    public GameObject tournamentPanel;
    public GameObject offlinePanel;
    public GameObject shopPanel;
    public GameObject cuesPanel;
    public GameObject rewardsPanel;
    public GameObject leaderboardsPanel;

    // Open a specific panel
    public void OpenPanel(string panelName)
    {
        GameObject panel = GetPanel(panelName);
        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning("Panel not found: " + panelName);
    }

    // Close a specific panel
    public void ClosePanel(string panelName)
    {
        GameObject panel = GetPanel(panelName);
        if (panel != null)
            panel.SetActive(false);
        else
            Debug.LogWarning("Panel not found: " + panelName);
    }

    // Toggle a specific panel
    public void TogglePanel(string panelName)
    {
        GameObject panel = GetPanel(panelName);
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
        else
            Debug.LogWarning("Panel not found: " + panelName);
    }

    // Helper function to get panel by name
    private GameObject GetPanel(string panelName)
    {
        switch (panelName)
        {
            case "PoolPass": return poolPassPanel;
            case "Chat": return chatPanel;
            case "Friends": return friendsPanel;
            case "Settings": return settingsPanel;
            case "Events": return eventsPanel;
            case "Play1v1": return play1v1Panel;
            case "PlayWithFriends": return playWithFriendsPanel;
            case "Tournament": return tournamentPanel;
            case "Offline": return offlinePanel;
            case "Shop": return shopPanel;
            case "Cues": return cuesPanel;
            case "Rewards": return rewardsPanel;
            case "Leaderboards": return leaderboardsPanel;
            default: return null;
        }
    }

    // Close all panels (optional)
    public void CloseAllPanels()
    {
        poolPassPanel.SetActive(false);
        chatPanel.SetActive(false);
        friendsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        eventsPanel.SetActive(false);
        play1v1Panel.SetActive(false);
        playWithFriendsPanel.SetActive(false);
        tournamentPanel.SetActive(false);
        offlinePanel.SetActive(false);
        shopPanel.SetActive(false);
        cuesPanel.SetActive(false);
        rewardsPanel.SetActive(false);
        leaderboardsPanel.SetActive(false);
    }
}

