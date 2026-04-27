using UnityEngine;

public class WagerManager : MonoBehaviour
{
    public PlayerWallet Wallet;

    [Header("Wager")]
    public int AntePerPlayer = 100; // each player puts this in at match start
    public int PotCoins { get; private set; }

    public bool StartWager()
    {
        PotCoins = 0;

        if (Wallet == null) return false;

        // Both players must pay ante
        if (!Wallet.CanAfford(0, AntePerPlayer) || !Wallet.CanAfford(1, AntePerPlayer))
            return false;

        Wallet.TrySpend(0, AntePerPlayer);
        Wallet.TrySpend(1, AntePerPlayer);

        PotCoins = AntePerPlayer * 2;
        return true;
    }

    public void AwardPotToWinner(int winnerIndex)
    {
        if (Wallet == null) return;
        Wallet.AddCoins(winnerIndex, PotCoins);
        PotCoins = 0;
    }

    public void ResetPot()
    {
        PotCoins = 0;
    }

    // Optional: raise wager mid-match
    public bool TryAddToPot(int playerIndex, int amount)
    {
        if (Wallet == null) return false;
        if (!Wallet.TrySpend(playerIndex, amount)) return false;
        PotCoins += amount;
        return true;
    }
}