using UnityEngine;

public class ToggleMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;

    private bool isOpen = false;

    public void Toggle()
    {
        isOpen = !isOpen;
        menuPanel.SetActive(isOpen);
    }
}