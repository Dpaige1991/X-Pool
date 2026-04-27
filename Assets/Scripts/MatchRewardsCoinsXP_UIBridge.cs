using UnityEngine;

public class MatchRewardsCoinsXP_UIBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TurnManager turnManager;

    [Header("Currency UI (assign if you want per-player UI updates)")]
    [Tooltip("If you have separate HUDs per player, assign both.")]
    [SerializeField] private CurrencyUIController player1UI;
    [SerializeField] private CurrencyUIController player2UI;

    [Tooltip("If you only have ONE HUD in your scene, assign it here and leave player1UI/player2UI empty.\nThis will show rewards on that single HUD (winner coins + winner/loser XP sequentially).")]
    [SerializeField] private CurrencyUIController singleUI;

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
            Debug.LogError("[MatchRewardsCoinsXP_UIBridge] No TurnManager found. Assign it or put this on the same GameObject.");
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

    // --------------------
    // Pot API
    // --------------------
    public void ResetPot()
    {
        PotAmount = Mathf.Max(0, startingPot);
        _paidThisGame = false;

        if (debugLogs)
            Debug.Log($"[MatchRewardsCoinsXP_UIBridge] Pot reset: {PotAmount}");
    }

    public void AddToPot(int amount)
    {
        if (amount <= 0) return;
        PotAmount += amount;

        if (debugLogs)
            Debug.Log($"[MatchRewardsCoinsXP_UIBridge] AddToPot +{amount} => Pot={PotAmount}");
    }

    public void SetPot(int amount)
    {
        PotAmount = Mathf.Max(0, amount);

        if (debugLogs)
            Debug.Log($"[MatchRewardsCoinsXP_UIBridge] SetPot => Pot={PotAmount}");
    }

    // --------------------
    // Game Over -> Rewards
    // --------------------
    private void HandleGameOver(GameOverInfo info)
    {
        if (_paidThisGame) return;
        if (!info.GameOver) return;

        PlayerId winner = info.ShooterWins ? info.Shooter : Other(info.Shooter);
        PlayerId loser = Other(winner);

        int coinsPayout = PotAmount;

        if (debugLogs)
            Debug.Log($"[MatchRewardsCoinsXP_UIBridge] GameOver => Winner={winner} Loser={loser} | CoinsPot={coinsPayout} | XP(W/L)={winnerXP}/{loserXP}");

        // 1) COINS -> Winner (and animate UI)
        if (coinsPayout > 0)
        {
            AwardCoins(winner, coinsPayout);
            AwardCoinsUI(winner, coinsPayout);
        }

        // 2) XP -> Winner + Loser (and animate UI)
        if (winnerXP > 0)
        {
            AwardXP(winner, winnerXP);
            AwardXPUI(winner, winnerXP);
        }

        if (loserXP > 0)
        {
            AwardXP(loser, loserXP);
            AwardXPUI(loser, loserXP);
        }

        _paidThisGame = true;

        if (resetPotAfterPayout) ResetPot();
        else PotAmount = 0;
    }

    // --------------------
    // Hooks to YOUR systems
    // --------------------
    private void AwardCoins(PlayerId player, int amount)
    {
        // If you already have your own coin backend, replace this section.
        if (CoinManager.Instance == null)
        {
            if (debugLogs) Debug.LogWarning("[MatchRewardsCoinsXP_UIBridge] CoinManager.Instance is null. Coins not stored anywhere (UI may still animate).");
            return;
        }

        if (player == PlayerId.Player1) CoinManager.Instance.AddCoinsToPlayer1(amount);
        else CoinManager.Instance.AddCoinsToPlayer2(amount);
    }

    private void AwardXP(PlayerId player, int amount)
    {
        // If you already have your own XP/level backend, replace this section.
        if (ExperienceManager.Instance == null)
        {
            if (debugLogs) Debug.LogWarning("[MatchRewardsCoinsXP_UIBridge] ExperienceManager.Instance is null. XP not stored anywhere (UI may still animate).");
            return;
        }

        if (player == PlayerId.Player1) ExperienceManager.Instance.AddXPToPlayer1(amount);
        else ExperienceManager.Instance.AddXPToPlayer2(amount);
    }

    // --------------------
    // UI Animation via CurrencyUIController
    // --------------------
    private void AwardCoinsUI(PlayerId player, int amount)
    {
        var ui = GetUIForPlayer(player);
        if (ui != null)
        {
            ui.AddCoins(amount);
            return;
        }

        // Single HUD fallback
        if (singleUI != null)
            singleUI.AddCoins(amount);
    }

    private void AwardXPUI(PlayerId player, int amount)
    {
        var ui = GetUIForPlayer(player);
        if (ui != null)
        {
            ui.AddExperience(amount);
            return;
        }

        // Single HUD fallback
        if (singleUI != null)
            singleUI.AddExperience(amount);
    }

    private CurrencyUIController GetUIForPlayer(PlayerId p)
    {
        // Prefer per-player HUD if assigned
        if (p == PlayerId.Player1 && player1UI != null) return player1UI;
        if (p == PlayerId.Player2 && player2UI != null) return player2UI;
        return null;
    }

    private PlayerId Other(PlayerId p) => p == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
}