using UnityEngine;
using UnityEngine.UI;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private Image avatarImage;

    void Start()
    {
        LoadAvatar();
    }

    public void LoadAvatar()
    {
        string savedSpriteName = PlayerDataManager.Instance.CurrentPlayerData.avatarSprite;

        if (string.IsNullOrEmpty(savedSpriteName))
        {
            Debug.LogWarning("No saved avatar sprite name found.");
            return;
        }

        Sprite loadedSprite = Resources.Load<Sprite>("Avatars/" + savedSpriteName);

        if (loadedSprite != null)
        {
            avatarImage.sprite = loadedSprite;
        }
        else
        {
            Debug.LogError("Sprite not found in Resources/Avatars: " + savedSpriteName);
        }
    }
}
