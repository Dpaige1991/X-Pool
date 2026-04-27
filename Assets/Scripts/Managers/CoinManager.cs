using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int Player1Coins;
    public int Player2Coins;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCoinsToPlayer1(int amount)
    {
        Player1Coins += amount;
        Debug.Log($"Player 1 coins: {Player1Coins}");
    }

    public void AddCoinsToPlayer2(int amount)
    {
        Player2Coins += amount;
        Debug.Log($"Player 2 coins: {Player2Coins}");
    }
}