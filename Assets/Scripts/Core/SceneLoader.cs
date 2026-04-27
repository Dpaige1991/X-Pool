using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadMatchmakingScene()
    {
        SceneManager.LoadScene("ShopScene");
    }
}