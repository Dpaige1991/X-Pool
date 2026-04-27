using UnityEngine;

public class MatchRewardsCoinsXP : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TurnManager turnManager;

    [Header("Pot (Coins)")]
    [SerializeField] private int startingPot = 1000;
    [SerializeField] private bool resetPotAfterPayout = true;

    [Header("XP Rewards")]
    [SerializeField] private int winnerXP = 100;
    [SerializeField] private int loserXP = 40;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public int PotAmount { get; private set; }

    private bool _paidThisGame;

    private void Awake()
    {
        if (turnManager == null) turnManager = GetComponent<TurnManager>();
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();

        if (turnManager == null)
            Debug.LogError("[MatchRewardsCoinsXP] No TurnManager found. Assign it or put this on the same GameObject.");
    }

    private void OnEnable()
    {
        if (turnManager != null)
            turnManager.OnGameOverUI += HandleGameOver;

        ResetPot();
    }

    private void OnDisable()
    {
        if (turnManager != null)
            turnManager.OnGameOverUI -= HandleGameOver;
    }

    public void ResetPot()
    {
        PotAmount = Mathf.Max(0, startingPot);
        _paidThisGame = false;

        if (debugLogs)
            Debug.Log($"[MatchRewardsCoinsXP] Pot reset: {PotAmount}");
    }

    public void AddToPot(int amount)
    {
        if (amount <= 0) return;
        PotAmount += amount;

        if (debugLogs)
            Debug.Log($"[MatchRewardsCoinsXP] AddToPot +{amount} => Pot={PotAmount}");
    }

    public void SetPot(int amount)
    {
        PotAmount = Mathf.Max(0, amount);

        if (debugLogs)
            Debug.Log($"[MatchRewardsCoinsXP] SetPot => Pot={PotAmount}");
    }

    private void HandleGameOver(GameOverInfo info)
    {
        if (_paidThisGame) return;
        if (!info.GameOver) return;

        PlayerId winner = info.ShooterWins ? info.Shooter : Other(info.Shooter);
        PlayerId loser = Other(winner);

        // 1) Coins to winner
        int payout = PotAmount;
        if (payout > 0)
        {
            AwardCoins(winner, payout);
        }

        // 2) XP to winner + loser
        AwardXP(winner, winnerXP);
        AwardXP(loser, loserXP);

        if (debugLogs)
        {
            Debug.Log($"[MatchRewardsCoinsXP] GameOver => Winner={winner} Loser={loser} | Coins={payout} | XP(W/L)={winnerXP}/{loserXP}");
        }

        _paidThisGame = true;

        if (resetPotAfterPayout) ResetPot();
        else PotAmount = 0;
    }

    private void AwardCoins(PlayerId player, int amount)
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogWarning("[MatchRewardsCoinsXP] CoinManager.Instance is null. Coins not awarded.");
            return;
        }

        if (player == PlayerId.Player1) CoinManager.Instance.AddCoinsToPlayer1(amount);
        else CoinManager.Instance.AddCoinsToPlayer2(amount);
    }

    private void AwardXP(PlayerId player, int amount)
    {
        if (amount <= 0) return;

        if (ExperienceManager.Instance == null)
        {
            Debug.LogWarning("[MatchRewardsCoinsXP] ExperienceManager.Instance is null. XP not awarded.");
            return;
        }

        if (player == PlayerId.Player1) ExperienceManager.Instance.AddXPToPlayer1(amount);
        else ExperienceManager.Instance.AddXPToPlayer2(amount);
    }

    private PlayerId Other(PlayerId p) => p == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
}