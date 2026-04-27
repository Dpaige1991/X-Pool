using System;
using System.Collections.Generic;
using UnityEngine;

public class BallsStoppedDetector : MonoBehaviour
{
    [Header("Auto-find")]
    public string ballTag = "Ball";

    [Tooltip("Optional: assign cue ball Rigidbody here. If null, it will try to find by cueBallTag.")]
    public Rigidbody cueBall;

    public string cueBallTag = "CueBall";

    [Header("Stop thresholds")]
    public float linearSpeedThreshold = 0.03f;   // m/s
    public float angularSpeedThreshold = 0.15f;  // rad/s

    [Header("Stability")]
    public float settleTime = 0.35f;
    public float checkInterval = 0.05f;

    [Header("DEBUG")]
    public bool debugLogs = true;
    public bool debugVelocities = false;
    public bool debugOnScreen = true;

    public event Action OnAllBallsStopped;
    public bool AreBallsStopped { get; private set; }

    readonly List<Rigidbody> _balls = new List<Rigidbody>(32);

    float _settleTimer;
    float _nextCheckTime;
    bool _fired;
    bool _wasMovingLastCheck;

    void Awake()
    {
        RefreshBallList();
    }

    public void RefreshBallList()
    {
        _balls.Clear();

        var balls = GameObject.FindGameObjectsWithTag(ballTag);
        foreach (var b in balls)
        {
            var rb = b.GetComponent<Rigidbody>();
            if (rb != null) _balls.Add(rb);
        }

        if (cueBall == null)
        {
            var cueGo = GameObject.FindGameObjectWithTag(cueBallTag);
            if (cueGo != null) cueBall = cueGo.GetComponent<Rigidbody>();
        }

        if (cueBall != null && !_balls.Contains(cueBall))
            _balls.Add(cueBall);

        if (debugLogs)
            Debug.Log($"[BallsStoppedDetector] Found {_balls.Count} rigidbodies (Ball tag + cue).");

        AreBallsStopped = false;
        _settleTimer = 0f;
        _nextCheckTime = 0f;
        _fired = false;
        _wasMovingLastCheck = false;
    }

    void Update()
    {
        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + checkInterval;

        bool allStoppedNow = AllStoppedRightNow();

        if (!allStoppedNow && !_wasMovingLastCheck)
        {
            if (debugLogs) Debug.Log("<color=yellow>[BallsStoppedDetector] Ball movement detected.</color>");
        }

        if (allStoppedNow)
        {
            _settleTimer += checkInterval;

            if (_settleTimer >= settleTime)
            {
                AreBallsStopped = true;

                if (!_fired)
                {
                    _fired = true;
                    if (debugLogs) Debug.Log("<color=green>[BallsStoppedDetector] All balls stopped.</color>");
                    OnAllBallsStopped?.Invoke();
                }
            }
        }
        else
        {
            AreBallsStopped = false;
            _settleTimer = 0f;
            _fired = false;
        }

        _wasMovingLastCheck = !allStoppedNow;
    }

    bool AllStoppedRightNow()
    {
        if (_balls.Count == 0) return true;

        float linSqr = linearSpeedThreshold * linearSpeedThreshold;
        float angSqr = angularSpeedThreshold * angularSpeedThreshold;

        for (int i = 0; i < _balls.Count; i++)
        {
            var rb = _balls[i];
            if (!rb) continue;

            if (debugVelocities)
                Debug.Log($"{rb.name} | Vel: {rb.linearVelocity.magnitude:F4} | Ang: {rb.angularVelocity.magnitude:F4}");

            // Rely on thresholds (more reliable than IsSleeping() across physics settings)
            if (rb.linearVelocity.sqrMagnitude > linSqr) return false;
            if (rb.angularVelocity.sqrMagnitude > angSqr) return false;
        }

        return true;
    }

    void OnGUI()
    {
        if (!debugOnScreen) return;
        GUI.Label(new Rect(10, 10, 700, 24),
            $"Detector | AreBallsStopped: {AreBallsStopped} | settle: {_settleTimer:F2}/{settleTime:F2} | balls: {_balls.Count}");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = AreBallsStopped ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}