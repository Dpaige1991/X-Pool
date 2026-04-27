using System;
using System.Collections.Generic;
using UnityEngine;

public enum _PocketId
{
    Unknown,
    Corner1, Corner2, Corner3, Corner4,
    Side1, Side2
}

[DefaultExecutionOrder(-100)] // ensures Awake runs before most other scripts
public class ShotTracker : MonoBehaviour
{
    public static ShotTracker Instance { get; private set; }

    public event Action<Ball, _PocketId> OnBallPocketed;
    public event Action<IReadOnlyList<Ball>> OnShotResolved;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<Ball> _pocketedThisShot = new();

    // shot gating
    public bool ShotActive { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (debugLogs)
                Debug.LogWarning($"[ShotTracker] Duplicate detected. Destroying: {name} (id={GetInstanceID()}) Keeping: {Instance.name} (id={Instance.GetInstanceID()})");

            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (debugLogs)
            Debug.Log($"[ShotTracker] Awake set Instance (id={GetInstanceID()}) active={gameObject.activeInHierarchy} enabled={enabled}");
    }

    private void OnDestroy()
    {
        // Important: prevent stale singleton reference if scene reloads / object destroyed
        if (Instance == this)
        {
            Instance = null;
            if (debugLogs)
                Debug.Log($"[ShotTracker] OnDestroy cleared Instance (id={GetInstanceID()})");
        }
    }

    /// <summary>Call when the player actually strikes the cue ball (the shot starts).</summary>
    public void BeginShot()
    {
        ShotActive = true;
        _pocketedThisShot.Clear();

        if (debugLogs)
            Debug.Log($"[ShotTracker] BeginShot (id={GetInstanceID()})");
    }

    /// <summary>Optional: call if you want to abort a shot (re-rack, reset, etc).</summary>
    public void CancelShot()
    {
        ShotActive = false;
        _pocketedThisShot.Clear();

        if (debugLogs)
            Debug.Log($"[ShotTracker] CancelShot (id={GetInstanceID()})");
    }

    public void RegisterPocket(Ball ball, _PocketId pocketId)
    {
        if (ball == null) return;

        // ignore pockets outside of an active shot
        if (!ShotActive)
        {
            if (debugLogs)
                Debug.LogWarning($"[ShotTracker] RegisterPocket ignored (no active shot). Ball={ball.name} Pocket={pocketId}");
            return;
        }

        _pocketedThisShot.Add(ball);
        OnBallPocketed?.Invoke(ball, pocketId);

        if (debugLogs)
            Debug.Log($"[ShotTracker] Pocketed: {ball.name} (cue={ball.IsCue}, eight={ball.IsEight}) -> {pocketId}");
    }

    /// <summary>Call when all balls have stopped to resolve the shot.</summary>
    public void ResolveShot()
    {
        // prevent phantom resolves at match start
        if (!ShotActive)
        {
            if (debugLogs)
                Debug.LogWarning("[ShotTracker] ResolveShot ignored (no active shot). Likely initial table settle.");
            return;
        }

        ShotActive = false;

        if (debugLogs)
            Debug.Log($"[ShotTracker] ResolveShot pocketedCount={_pocketedThisShot.Count}");

        OnShotResolved?.Invoke(_pocketedThisShot);
        _pocketedThisShot.Clear();
    }
}