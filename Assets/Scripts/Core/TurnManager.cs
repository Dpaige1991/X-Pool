using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerId { Player1, Player2 }

public enum GameOverReason
{
    EightBallLegalWin,
    EightBallIllegal,
}

[Serializable]
public struct GameOverInfo
{
    public bool GameOver;
    public PlayerId Shooter;
    public bool ShooterWins;
    public GameOverReason Reason;
    public bool Foul;
    public bool ClearedGroup;

    public string Title;
    public string Detail;
}

public class TurnManager : MonoBehaviour
{
    [Header("Auto Start")]
    [SerializeField] private bool autoStartOnPlay = true;

    [Header("State (Inspector values are ignored once match starts)")]
    [SerializeField] private PlayerId currentPlayer = PlayerId.Player1;
    [SerializeField] private PlayerId breakPlayer = PlayerId.Player1;

    public PlayerId CurrentPlayer => currentPlayer;
    public PlayerId BreakPlayer => breakPlayer;

    // Player assignments
    public BallGroup P1Group { get; private set; } = BallGroup.None;
    public BallGroup P2Group { get; private set; } = BallGroup.None;
    public bool TableOpen => P1Group == BallGroup.None && P2Group == BallGroup.None;

    // Turn flags
    public bool IsBreakShot { get; private set; }
    public bool BallInHand { get; private set; }

    [Header("Player Info")]
    [SerializeField] private string player1Name = "Player 1";
    [SerializeField] private string player2Name = "Player 2";
    public string Player1Name => player1Name;
    public string Player2Name => player2Name;

    // UI Events
    public event Action<IReadOnlyList<Ball>> OnShotResolvedUI;
    public event Action<BallGroup, BallGroup> OnGroupsAssignedUI;
    public event Action<GameOverInfo> OnGameOverUI;
    public event Action<PlayerId> OnTurnBeganUI;
    public event Action<PlayerId> OnTurnEndedUI;
    public event Action OnMatchEndedUI;

    [Header("Ball-In-Hand Integration")]
    public CueBallScratchHandler scratchHandler;
    public CueBallPlacement cueBallPlacement;
    public Ball cueBallBall;
    public Rigidbody cueBallRb;

    private bool _bihPlacementStartedThisTurn;
    private bool _matchStarted;
    private bool _shotTakenThisTurn;

    // ✅ NEW: cached shot tracker ref (prevents OnEnable null issues)
    private ShotTracker _shotTracker;

    private bool _scratchHandledThisShot;
    private bool _ignoreNextResolve; // prevents double-switch when balls stop

    private void Awake()
    {
        // --- ShotTracker hookup (robust) ---
        _shotTracker = ShotTracker.Instance;
        if (_shotTracker == null)
            _shotTracker = FindObjectOfType<ShotTracker>(true); // finds even if inactive

        if (_shotTracker == null)
            Debug.LogError("[TurnManager] No ShotTracker found in scene (even inactive). Add ShotTracker to a GameObject.");
        else
            Debug.Log($"[TurnManager] Found ShotTracker id={_shotTracker.GetInstanceID()} active={_shotTracker.gameObject.activeInHierarchy} enabled={_shotTracker.enabled}");

        if (scratchHandler == null)
            scratchHandler = CueBallScratchHandler.Instance;

        if (cueBallPlacement == null)
            cueBallPlacement = FindFirstObjectByType<CueBallPlacement>();

        if (cueBallBall == null)
        {
            var cueGo = GameObject.FindGameObjectWithTag("CueBall");
            if (cueGo != null)
                cueBallBall = cueGo.GetComponentInParent<Ball>() ?? cueGo.GetComponentInChildren<Ball>() ?? cueGo.GetComponent<Ball>();
        }

        if (cueBallRb == null)
        {
            if (cueBallBall != null)
            {
                cueBallRb =
                    cueBallBall.GetComponent<Rigidbody>() ??
                    cueBallBall.GetComponentInChildren<Rigidbody>() ??
                    cueBallBall.GetComponentInParent<Rigidbody>();
            }
            else
            {
                var cueGo = GameObject.FindGameObjectWithTag("CueBall");
                if (cueGo != null) cueBallRb = cueGo.GetComponent<Rigidbody>();
            }
        }
    }

    private void Start()
    {
        if (autoStartOnPlay && !_matchStarted)
            StartMatchRandomBreak();
    }

    private void OnEnable()
    {
        Debug.Log($"[TurnManager] OnEnable id={GetInstanceID()} enabled={enabled} active={gameObject.activeInHierarchy}");

        if (_shotTracker == null)
            _shotTracker = ShotTracker.Instance ?? FindObjectOfType<ShotTracker>(true);

        if (_shotTracker != null)
        {
            _shotTracker.OnShotResolved += HandleShotResolved;
            _shotTracker.OnBallPocketed += HandleBallPocketed;   // ✅ NEW
            Debug.Log("[TurnManager] Subscribed to ShotTracker.OnShotResolved + OnBallPocketed");
        }
        else
        {
            Debug.LogError("[TurnManager] ShotTracker is NULL in OnEnable (no instance found).");
        }
    }

    private void OnDisable()
    {
        Debug.Log($"[TurnManager] OnDisable id={GetInstanceID()}");

        if (_shotTracker != null)
        {
            _shotTracker.OnShotResolved -= HandleShotResolved;
            _shotTracker.OnBallPocketed -= HandleBallPocketed;  // ✅ NEW
        }
    }

    // ---------- Match Flow ----------

    public void StartMatchRandomBreak()
    {
        breakPlayer = UnityEngine.Random.value < 0.5f ? PlayerId.Player1 : PlayerId.Player2;
        StartMatchWithBreak(breakPlayer);
    }

    public void StartMatchWithBreak(PlayerId breaker)
    {
        _matchStarted = true;

        breakPlayer = breaker;

        // reset state
        P1Group = BallGroup.None;
        P2Group = BallGroup.None;
        BallInHand = false;

        // breaker is ALWAYS the current player at match start
        currentPlayer = breakPlayer;

        IsBreakShot = true;
        _bihPlacementStartedThisTurn = false;

        Debug.Log($"[TurnManager] Match Start id={GetInstanceID()} | Breaker={breakPlayer} | Current={currentPlayer}");

        BeginTurn();
    }

    public void BeginTurn()
    {
        _bihPlacementStartedThisTurn = false;
        _shotTakenThisTurn = false;

        Debug.Log($"[TurnManager] BeginTurn id={GetInstanceID()} | Current={currentPlayer} | BreakShot={IsBreakShot} | BIH={BallInHand} | TableOpen={TableOpen}");
        OnTurnBeganUI?.Invoke(currentPlayer);

        if (BallInHand)
            TryStartBallInHandPlacement();
    }

    private void TryStartBallInHandPlacement()
    {
        if (_bihPlacementStartedThisTurn) return;
        _bihPlacementStartedThisTurn = true;

        if (cueBallRb == null)
        {
            Debug.LogWarning("[TurnManager] BallInHand active but cueBallRb is null. Cannot start placement.");
            return;
        }

        if (cueBallPlacement == null)
        {
            Debug.LogWarning("[TurnManager] BallInHand active but CueBallPlacement not assigned/found. Cannot start placement.");
            return;
        }

        Debug.Log("<color=yellow>[TurnManager]</color> Ball-In-Hand: starting cue ball placement.");
        cueBallPlacement.BeginPlacement(cueBallRb);
    }

    private void HandleBallPocketed(Ball ball, _PocketId pocketId)
    {
        if (ball == null) return;

        // Only care about cue ball scratches
        if (!ball.IsCue) return;

        // Only during a real shot
        if (!_shotTakenThisTurn) return;

        // Prevent double-processing if cue ball collides with multiple triggers
        if (_scratchHandledThisShot) return;

        _scratchHandledThisShot = true;

        Debug.Log($"<color=red>[TurnManager]</color> Cue ball pocketed DURING shot -> immediate foul turn switch. Pocket={pocketId}");

        // Park/freeze cue ball immediately (visual feedback)
        if (scratchHandler == null) scratchHandler = CueBallScratchHandler.Instance;
        if (scratchHandler != null && cueBallBall != null)
            scratchHandler.HandleScratch(cueBallBall);

        // Give opponent BIH and switch RIGHT NOW (timer starts for opponent immediately)
        BallInHand = true;

        // IMPORTANT: when balls stop later and ResolveShot fires, ignore it so we don’t switch twice
        _ignoreNextResolve = true;

        _shotTakenThisTurn = false; // consume this turn immediately so nothing else treats it as active

        EndTurnSwitchPlayer();
    }

    public void ConfirmBallInHandPlaced()
    {
        if (!BallInHand) return;

        BallInHand = false;
        Debug.Log("<color=yellow>[TurnManager]</color> Ball-In-Hand consumed (placement confirmed).");
    }

    private void EndTurnSwitchPlayer()
    {
        Debug.Log($"[TurnManager] EndTurnSwitchPlayer ENTER id={GetInstanceID()} currentPlayer(before)={currentPlayer}");

        OnTurnEndedUI?.Invoke(currentPlayer);
        PlayerStats.Instance?.OnTurnEnded(ToStatsPlayer(currentPlayer));

        currentPlayer = Other(currentPlayer);

        Debug.Log($"[TurnManager] EndTurnSwitchPlayer EXIT id={GetInstanceID()} currentPlayer(after)={currentPlayer}");

        IsBreakShot = false;
        BeginTurn();
    }

    public void ForceTurnTimeout()
    {
        Debug.Log($"<color=orange>[Timeout]</color> {currentPlayer} took too long. Switching turn.");
        BallInHand = false;
        EndTurnSwitchPlayer();
    }

    private PlayerId Other(PlayerId p) => p == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
    private BallGroup GetPlayerGroup(PlayerId p) => p == PlayerId.Player1 ? P1Group : P2Group;
    private _PlayerId ToStatsPlayer(PlayerId p) => (p == PlayerId.Player1) ? _PlayerId.Player1 : _PlayerId.Player2;

    // ---------- Core Rules ----------

    public void NotifyShotTaken()
    {
        _shotTakenThisTurn = true;
        _scratchHandledThisShot = false;
        _ignoreNextResolve = false;

        Debug.Log($"[TurnManager] Shot taken by {currentPlayer}");
    }

    private void HandleShotResolved(IReadOnlyList<Ball> pocketed)
    {
        if (_ignoreNextResolve)
        {
            Debug.LogWarning("[TurnManager] Ignoring ShotResolved because scratch already switched the turn immediately.");
            _ignoreNextResolve = false;
            _shotTakenThisTurn = false; // consume shot
            return;
        }

        Debug.Log($"[TurnManager] HandleShotResolved CALLED. id={GetInstanceID()} currentPlayer={currentPlayer} pocketedCount={(pocketed == null ? -1 : pocketed.Count)}");

        // ignore phantom resolves
        if (!_shotTakenThisTurn)
        {
            Debug.LogWarning($"[TurnManager] Ignoring ShotResolved because no shot was taken yet. Current={currentPlayer} (likely initial settle)");
            return;
        }

        _shotTakenThisTurn = false;

        if (pocketed == null) pocketed = Array.Empty<Ball>();

        bool cuePocketed = false;
        bool eightPocketed = false;

        bool pocketedAnySolid = false;
        bool pocketedAnyStripe = false;
        bool pocketedAnyObjectBall = false; // NEW

        bool pocketedOwnGroupBall = false;
        var shooterGroup = GetPlayerGroup(currentPlayer);

        for (int i = 0; i < pocketed.Count; i++)
        {
            var b = pocketed[i];
            if (b == null) continue;

            if (b.IsCue) cuePocketed = true;
            if (b.IsEight) eightPocketed = true;

            var g = b.GetGroup();
            if (g == BallGroup.Solids) pocketedAnySolid = true;
            if (g == BallGroup.Stripes) pocketedAnyStripe = true;

            if (!b.IsCue && !b.IsEight)
                pocketedAnyObjectBall = true;

            if (shooterGroup != BallGroup.None && g == shooterGroup)
                pocketedOwnGroupBall = true;

            Debug.Log($"[TurnManager] Pocketed: {b.name} BallNumber={b.BallNumber} IsCue={b.IsCue} IsEight={b.IsEight} Group={b.GetGroup()}");
        }

        bool foul = cuePocketed;

        // stats
        var ps = PlayerStats.Instance;
        if (ps != null)
        {
            var shooterStats = ToStatsPlayer(currentPlayer);
            bool pocketedAnyNonCue = false;

            for (int i = 0; i < pocketed.Count; i++)
            {
                var b = pocketed[i];
                if (b == null) continue;

                if (!b.IsCue)
                {
                    pocketedAnyNonCue = true;
                    ps.RegisterBallPocketed(shooterStats);
                }
            }

            if (foul) ps.RegisterFoul(shooterStats);
            else if (pocketedAnyNonCue) ps.RegisterSuccessfulShot(shooterStats);
            else ps.RegisterMiss(shooterStats);
        }

        // Assign groups if table open AND exactly one type pocketed
        if (TableOpen)
        {
            if (pocketedAnySolid ^ pocketedAnyStripe)
            {
                var assigned = pocketedAnySolid ? BallGroup.Solids : BallGroup.Stripes;
                AssignGroupsToCurrentPlayer(assigned);
                OnGroupsAssignedUI?.Invoke(P1Group, P2Group);

                shooterGroup = GetPlayerGroup(currentPlayer);
                pocketedOwnGroupBall = true; // assigning shot guarantees "own group" pocket
            }
            // If both types pocketed: do NOT assign; table stays open.
        }

        OnShotResolvedUI?.Invoke(pocketed);

        // 8-ball handling
        if (eightPocketed)
        {
            bool shooterCleared = HasClearedGroup(currentPlayer);
            bool legalWin = (!foul && shooterCleared);

            var info = new GameOverInfo
            {
                GameOver = true,
                Shooter = currentPlayer,
                ShooterWins = legalWin,
                Reason = legalWin ? GameOverReason.EightBallLegalWin : GameOverReason.EightBallIllegal,
                Foul = foul,
                ClearedGroup = shooterCleared,
                Title = "GAME OVER",
                Detail = legalWin
                    ? $"{(currentPlayer == PlayerId.Player1 ? player1Name : player2Name)} wins!"
                    : $"{(currentPlayer == PlayerId.Player1 ? player1Name : player2Name)} loses! {(currentPlayer == PlayerId.Player1 ? player2Name : player1Name)} wins!"
            };

            OnGameOverUI?.Invoke(info);
            OnMatchEndedUI?.Invoke();
            return;
        }

        // BREAK HOUSE RULE:
        // If you scratch on the break but pocket any object ball, you KEEP shooting.
        // You also get Ball-In-Hand for YOURSELF to place the cue ball.
        if (IsBreakShot && foul && pocketedAnyObjectBall)
        {
            Debug.Log($"{currentPlayer} BREAK HOUSE RULE: scratched but pocketed a ball. Shooter continues with Ball-In-Hand.");

            if (scratchHandler == null) scratchHandler = CueBallScratchHandler.Instance;

            if (scratchHandler != null && cueBallBall != null)
                scratchHandler.HandleScratch(cueBallBall);
            else
                Debug.LogWarning("[TurnManager] Break scratch rule triggered, but scratchHandler or cueBallBall is missing.");

            BallInHand = true;   // for SAME shooter
            IsBreakShot = false; // break is over now
            BeginTurn();         // same player continues
            return;
        }

        // Foul (normal): BIH to opponent + switch
        if (foul)
        {
            Debug.Log($"{currentPlayer} FOUL: cue ball pocketed. Opponent ball-in-hand.");

            if (scratchHandler == null) scratchHandler = CueBallScratchHandler.Instance;

            if (scratchHandler != null && cueBallBall != null)
                scratchHandler.HandleScratch(cueBallBall);
            else
                Debug.LogWarning("[TurnManager] Scratch detected, but scratchHandler or cueBallBall is missing.");

            BallInHand = true;
            EndTurnSwitchPlayer();
            return;
        }

        // CONTINUE / SWITCH LOGIC

        // While table is open: pocket ANY object ball => continue (even if both types were pocketed)
        if (TableOpen)
        {
            if (pocketedAnyObjectBall)
            {
                Debug.Log($"{currentPlayer} continues (table open + pocketed at least one object ball).");
                BallInHand = false;
                IsBreakShot = false;
                BeginTurn();
            }
            else
            {
                Debug.Log($"{currentPlayer} turn ends (table open + no object ball pocketed).");
                BallInHand = false;
                EndTurnSwitchPlayer();
            }
            return;
        }

        // After groups are assigned: continue only if pocketed own group
        if (pocketedOwnGroupBall)
        {
            Debug.Log($"{currentPlayer} continues (pocketed own group ball).");
            BallInHand = false;
            IsBreakShot = false;
            BeginTurn();
        }
        else
        {
            Debug.Log($"{currentPlayer} turn ends (no own-group pocket).");
            BallInHand = false;
            EndTurnSwitchPlayer();
        }
    }

    private void AssignGroupsToCurrentPlayer(BallGroup currentPlayerGroup)
    {
        var oppGroup = (currentPlayerGroup == BallGroup.Solids) ? BallGroup.Stripes : BallGroup.Solids;

        if (currentPlayer == PlayerId.Player1)
        {
            P1Group = currentPlayerGroup;
            P2Group = oppGroup;
        }
        else
        {
            P2Group = currentPlayerGroup;
            P1Group = oppGroup;
        }

        Debug.Log($"Groups assigned: P1={P1Group}, P2={P2Group}");
    }

    private bool HasClearedGroup(PlayerId player)
    {
        var group = GetPlayerGroup(player);
        if (group == BallGroup.None) return false;

        var balls = FindObjectsOfType<Ball>(includeInactive: false);
        foreach (var b in balls)
        {
            if (b == null) continue;
            if (b.IsCue || b.IsEight) continue;

            if (b.GetGroup() == group)
                return false;
        }
        return true;
    }
}