using System;
using UnityEngine;

/// <summary>
/// Attach this to the SAME GameObject as TurnManager (or assign TurnManager reference).
/// It maintains a pot amount and pays it to the match winner when TurnManager reports Game Over.
/// </summary>
public class MatchPotPayout : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TurnManager turnManager;

    [Header("Pot Settings")]
    [Tooltip("Initial pot when the match begins (or when you call ResetPot).")]
    [SerializeField] private int startingPot = 1000;

    [Tooltip("If true, pot resets to startingPot after a payout.")]
    [SerializeField] private bool resetAfterPayout = true;

    [Tooltip("If true, pot resets when a new match starts (call ResetPot manually or from your match start flow).")]
    [SerializeField] private bool resetOnEnable = true;

    [Header("Who Receives Money (optional demo wallets)")]
    [Tooltip("Optional: assign a wallet for Player 1 (or leave null if you use a different system).")]
    [SerializeField] private SimplePlayerWallet player1Wallet;

    [Tooltip("Optional: assign a wallet for Player 2 (or leave null if you use a different system).")]
    [SerializeField] private SimplePlayerWallet player2Wallet;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public int PotAmount { get; private set; }

    /// <summary>
    /// Optional event if you want UI/FX to react: (winnerId, amountPaid)
    /// </summary>
    public event Action<PlayerId, int> OnPotPaid;

    private bool _paidThisGame;

    private void Awake()
    {
        if (turnManager == null) turnManager = GetComponent<TurnManager>();
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();

        if (turnManager == null)
            Debug.LogError("[MatchPotPayout] No TurnManager found. Assign it in inspector or put this on same GameObject.");
    }

    private void OnEnable()
    {
        if (turnManager != null)
            turnManager.OnGameOverUI += HandleGameOver;

        if (resetOnEnable)
            ResetPot();
    }

    private void OnDisable()
    {
        if (turnManager != null)
            turnManager.OnGameOverUI -= HandleGameOver;
    }

    /// <summary>
    /// Call this when you want to start a new match pot.
    /// </summary>
    public void ResetPot()
    {
        PotAmount = Mathf.Max(0, startingPot);
        _paidThisGame = false;

        if (debugLogs)
            Debug.Log($"[MatchPotPayout] Pot reset. PotAmount={PotAmount}");
    }

    /// <summary>
    /// Add money to the pot (entry fee, side bets, etc.)
    /// </summary>
    public void AddToPot(int amount)
    {
        if (amount <= 0) return;

        PotAmount += amount;

        if (debugLogs)
            Debug.Log($"[MatchPotPayout] Added to pot: +{amount}. PotAmount={PotAmount}");
    }

    /// <summary>
    /// If you want to set the pot directly.
    /// </summary>
    public void SetPot(int amount)
    {
        PotAmount = Mathf.Max(0, amount);

        if (debugLogs)
            Debug.Log($"[MatchPotPayout] Pot set. PotAmount={PotAmount}");
    }

    private void HandleGameOver(GameOverInfo info)
    {
        // Prevent double-pay if multiple listeners / duplicate calls happen.
        if (_paidThisGame) return;

        if (!info.GameOver) return;

        // Determine winner:
        // In your TurnManager: ShooterWins means the shooter won (legal 8-ball).
        // If ShooterWins is false, the OTHER player wins.
        PlayerId winner = info.ShooterWins ? info.Shooter : Other(info.Shooter);

        int payout = PotAmount;

        if (debugLogs)
            Debug.Log($"[MatchPotPayout] GameOver received. Shooter={info.Shooter} ShooterWins={info.ShooterWins} => Winner={winner}. Paying Pot={payout}");

        PayWinner(winner, payout);

        _paidThisGame = true;

        OnPotPaid?.Invoke(winner, payout);

        if (resetAfterPayout)
            ResetPot();
        else
            PotAmount = 0; // usually you'd empty it after paying
    }

    private void PayWinner(PlayerId winner, int amount)
    {
        if (amount <= 0)
        {
            if (debugLogs) Debug.Log("[MatchPotPayout] Pot is 0. Nothing to pay.");
            return;
        }

        // OPTION A (demo): pay via wallets if assigned
        var wallet = (winner == PlayerId.Player1) ? player1Wallet : player2Wallet;

        if (wallet != null)
        {
            wallet.AddMoney(amount);

            if (debugLogs)
                Debug.Log($"[MatchPotPayout] Paid {amount} to {winner} via wallet '{wallet.name}'.");
        }
        else
        {
            // OPTION B: You plug in YOUR currency system here
            // Example:
            // MoneySystem.Instance.AddCoins(winner, amount);

            if (debugLogs)
                Debug.LogWarning($"[MatchPotPayout] No wallet assigned for {winner}. Hook your currency system in PayWinner(). Amount={amount} NOT deposited anywhere.");
        }
    }

    private PlayerId Other(PlayerId p) => p == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
}