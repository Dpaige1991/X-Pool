using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class Menu
    {
        public string name;
        public GameObject panel;
    }

    [Header("All Menus")]
    public List<Menu> menus = new List<Menu>();

    private Dictionary<string, GameObject> menuDictionary;

    private void Awake()
    {
        menuDictionary = new Dictionary<string, GameObject>();

        // Register all menus in dictionary
        foreach (Menu m in menus)
        {
            if (m.panel != null && !menuDictionary.ContainsKey(m.name))
            {
                menuDictionary.Add(m.name, m.panel);
                m.panel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Shows a menu by name, hides all others
    /// </summary>
    public void ShowMenu(string menuName)
    {
        foreach (var kvp in menuDictionary)
        {
            kvp.Value.SetActive(false);
        }

        if (menuDictionary.ContainsKey(menuName))
        {
            menuDictionary[menuName].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Menu '{menuName}' not found in MenuManager.");
        }
    }

    /// <summary>
    /// Quick function for button OnClick events
    /// </summary>
    public void OnMenuButtonClicked(string menuName)
    {
        ShowMenu(menuName);
    }
}
