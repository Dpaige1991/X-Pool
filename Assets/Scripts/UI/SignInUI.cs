using UnityEngine;

public class SignInUI : MonoBehaviour
{
    public void SignInWithApple()
    {
        PlayerDataManager.Instance.SignIn("Apple");
    }

    public void SignInWithEmail()
    {
        PlayerDataManager.Instance.SignIn("Email");
    }
}
