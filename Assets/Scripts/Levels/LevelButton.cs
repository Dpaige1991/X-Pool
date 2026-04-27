using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public string levelId;

    public void OnClick()
    {
        LevelManager.Instance.TryEnterLevel(levelId);
    }
}
