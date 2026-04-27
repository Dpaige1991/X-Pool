using UnityEngine;

public class ShotEndResolver : MonoBehaviour
{
    [Header("Refs")]
    public BallsStoppedDetector detector;
    public ShotTracker shotTracker;

    [Header("Shot In Progress Heuristic")]
    [Tooltip("If true, we only resolve when we previously saw motion, then stop.")]
    public bool requireMotionBeforeResolve = true;

    bool _sawMotionSinceLastResolve;

    void Awake()
    {
        if (detector == null) detector = FindFirstObjectByType<BallsStoppedDetector>();
        if (shotTracker == null) shotTracker = FindFirstObjectByType<ShotTracker>();
    }

    void OnEnable()
    {
        if (detector != null)
            detector.OnAllBallsStopped += HandleAllStopped;
    }

    void OnDisable()
    {
        if (detector != null)
            detector.OnAllBallsStopped -= HandleAllStopped;
    }

    void Update()
    {
        if (!requireMotionBeforeResolve || detector == null) return;

        // If balls are NOT stopped at any point, a shot is in progress
        if (!detector.AreBallsStopped)
            _sawMotionSinceLastResolve = true;
    }

    void HandleAllStopped()
    {
        if (shotTracker == null) return;

        if (requireMotionBeforeResolve && !_sawMotionSinceLastResolve)
        {
            // Prevent resolving a shot at match start / rack settle
            return;
        }

        _sawMotionSinceLastResolve = false;

        Debug.Log("<color=cyan>[ShotEndResolver]</color> Resolving shot (all balls stopped).");
        shotTracker.ResolveShot();
    }

    /// <summary>Call this right when the player strikes the cue ball (recommended).</summary>
    public void NotifyShotStarted()
    {
        _sawMotionSinceLastResolve = true;
    }
}