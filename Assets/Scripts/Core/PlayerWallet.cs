using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int[] Coins = new int[2] { 1000, 1000 }; // starting coins

    public bool CanAfford(int playerIndex, int amount) => Coins[playerIndex] >= amount;

    public bool TrySpend(int playerIndex, int amount)
    {
        if (!CanAfford(playerIndex, amount)) return false;
        Coins[playerIndex] -= amount;
        return true;
    }

    public void AddCoins(int playerIndex, int amount)
    {
        Coins[playerIndex] += Mathf.Max(0, amount);
    }
}