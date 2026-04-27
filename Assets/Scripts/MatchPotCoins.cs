using UnityEngine;

public class MatchPotCoins : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;

    [Header("Pot Settings")]
    [SerializeField] private int startingPot = 1000;

    [SerializeField] private bool resetAfterPayout = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public int PotAmount { get; private set; }

    private bool payoutDone;

    void Awake()
    {
        if (turnManager == null)
            turnManager = GetComponent<TurnManager>();

        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();
    }

    void OnEnable()
    {
        if (turnManager != null)
            turnManager.OnGameOverUI += HandleGameOver;

        ResetPot();
    }

    void OnDisable()
    {
        if (turnManager != null)
            turnManager.OnGameOverUI -= HandleGameOver;
    }

    public void ResetPot()
    {
        PotAmount = startingPot;
        payoutDone = false;

        if (debugLogs)
            Debug.Log($"[MatchPotCoins] Pot reset: {PotAmount}");
    }

    public void AddToPot(int amount)
    {
        if (amount <= 0) return;

        PotAmount += amount;

        if (debugLogs)
            Debug.Log($"[MatchPotCoins] Pot increased by {amount}. New pot = {PotAmount}");
    }

    private void HandleGameOver(GameOverInfo info)
    {
        if (payoutDone) return;
        if (!info.GameOver) return;

        PlayerId winner = info.ShooterWins ? info.Shooter : Other(info.Shooter);

        PayWinner(winner);

        payoutDone = true;

        if (resetAfterPayout)
            ResetPot();
        else
            PotAmount = 0;
    }

    private void PayWinner(PlayerId winner)
    {
        int payout = PotAmount;

        if (payout <= 0)
        {
            if (debugLogs)
                Debug.Log("[MatchPotCoins] Pot empty.");
            return;
        }

        if (debugLogs)
            Debug.Log($"[MatchPotCoins] Paying {payout} coins to {winner}");

        // ADD COINS TO PLAYER
        if (winner == PlayerId.Player1)
        {
            CoinManager.Instance.AddCoinsToPlayer1(payout);
        }
        else
        {
            CoinManager.Instance.AddCoinsToPlayer2(payout);
        }
    }

    private PlayerId Other(PlayerId p)
    {
        return p == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
    }
}