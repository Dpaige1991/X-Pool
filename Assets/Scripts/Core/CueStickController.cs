using System;
using UnityEngine;

public class CueStickController : MonoBehaviour
{
    public static CueStickController Instance { get; private set; }

    public event Action<bool> OnCueReadyChanged;
    public bool IsCueReady => isAiming;

    [Header("References")]
    public Rigidbody cueBallRb;
    public Transform cueStickPivot; // pivot that stays on cue ball
    public BallsStoppedDetector stoppedDetector;
    public ShotEndResolver shotEndResolver;

    [Header("Turn (for stats)")]
    [Tooltip("Optional. If assigned/found, stats will use the active player from TurnManager.")]
    public TurnManager turnManager;

    [Header("Aiming")]
    public Camera aimCamera;
    public LayerMask tableLayerMask;

    [Header("Power")]
    public float maxImpulse = 12f;
    public float maxPullBackDistance = 0.35f;

    [Header("Placement")]
    public float restDistanceFromBall = 0.22f;
    public float heightOffset = 0.02f;

    [Header("Debug")]
    public bool debugCue = true;

    [Header("Spin (English)")]
    [Tooltip("Spin X = left/right (-1..1), Spin Y = top/back (-1..1)")]
    public Vector2 spin = Vector2.zero;

    [Tooltip("How strong spin is applied at full power. Tune this.")]
    public float maxSpinTorque = 8f;

    [Tooltip("Optional: If true, clears spin after each shot like many casual games.")]
    public bool resetSpinAfterShot = false;

    [Header("Aim Rotation Tuning")]
    public float aimRotateSpeedDegPerSec = 200f;
    public float aimYawSmoothTime = 0.07f;
    private float _aimYawVel;

    [Header("Ball In Hand")]
    [Tooltip("Assign the visual root of the cue stick to hide/show. This should be the child under the pivot.")]
    public GameObject cueStickVisualRoot;
    public bool IsBallInHand { get; private set; }

    // State
    private Transform pivot;
    private bool isAiming = true;
    private bool isDraggingPower = false;
    private float currentPower01 = 0f;

    private Transform visualRootTransform;
    private Vector3 visualRestLocalPosition;
    private Quaternion visualRestLocalRotation;
    private Vector3 visualRestLocalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pivot = cueStickPivot != null ? cueStickPivot : transform;

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (cueBallRb == null)
        {
            GameObject cueBallGo = GameObject.FindGameObjectWithTag("CueBall");
            if (cueBallGo != null)
                cueBallRb = cueBallGo.GetComponent<Rigidbody>();
        }

        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (cueStickVisualRoot != null)
        {
            visualRootTransform = cueStickVisualRoot.transform;
            visualRestLocalPosition = visualRootTransform.localPosition;
            visualRestLocalRotation = visualRootTransform.localRotation;
            visualRestLocalScale = visualRootTransform.localScale;
        }
    }

    private void Start()
    {
        if (debugCue) Debug.Log("[CueStick] Start");

        if (stoppedDetector == null)
        {
            stoppedDetector = FindFirstObjectByType<BallsStoppedDetector>();
            if (debugCue) Debug.Log($"[CueStick] Auto-found detector? {(stoppedDetector != null)}");
        }

        if (stoppedDetector != null)
        {
            stoppedDetector.OnAllBallsStopped += HandleAllBallsStopped;
            if (debugCue) Debug.Log("[CueStick] Subscribed to OnAllBallsStopped");
        }
        else
        {
            Debug.LogError("[CueStick] No BallsStoppedDetector assigned/found. Cue will never re-arm.");
        }

        SnapPivotToCueBall();
        ResetVisualToRest();
        NotifyCueReady(isAiming);
    }

    private void OnDestroy()
    {
        if (stoppedDetector != null)
            stoppedDetector.OnAllBallsStopped -= HandleAllBallsStopped;

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (cueBallRb == null || pivot == null)
            return;

        if (IsBallInHand)
            return;

        if (isAiming)
        {
            SnapPivotToCueBall();

            if (Input.GetMouseButton(1))
                AimAtPointer();

            PlaceCueVisualAtRest();
            ApplyVisualPullBack();
        }
    }

    // --- UI calls ---
    public void SetPower01(float value01)
    {
        currentPower01 = Mathf.Clamp01(value01);
    }

    public void OnPowerDragBegin()
    {
        if (!isAiming || IsBallInHand)
            return;

        isDraggingPower = true;

        if (debugCue)
            Debug.Log("[CueStick] Power drag begin");
    }

    public void OnPowerDragEndAndShoot()
    {
        if (!isAiming || IsBallInHand)
            return;

        if (!isDraggingPower)
            return;

        isDraggingPower = false;

        if (currentPower01 <= 0.001f)
        {
            if (debugCue)
                Debug.Log("[CueStick] Released power drag with ~0 power. Not shooting.");
            return;
        }

        Shoot();
    }

    public void SetSpin(Vector2 newSpin)
    {
        spin = new Vector2(
            Mathf.Clamp(newSpin.x, -1f, 1f),
            Mathf.Clamp(newSpin.y, -1f, 1f)
        );

        if (debugCue)
            Debug.Log($"[CueStick] Spin set to {spin}");
    }

    public void ClearSpin()
    {
        spin = Vector2.zero;

        if (debugCue)
            Debug.Log("[CueStick] Spin cleared");
    }

    private _PlayerId GetActiveStatsPlayer()
    {
        if (turnManager != null)
            return (turnManager.CurrentPlayer == PlayerId.Player1)
                ? _PlayerId.Player1
                : _PlayerId.Player2;

        return _PlayerId.Player1;
    }

    private void Shoot()
    {
        if (IsBallInHand)
            return;

        ShotTracker.Instance?.BeginShot();
        turnManager?.NotifyShotTaken();
        shotEndResolver?.NotifyShotStarted();

        PlayerStats ps = PlayerStats.Instance;
        if (ps != null)
        {
            _PlayerId p = GetActiveStatsPlayer();
            ps.RegisterShot(p);

            if (debugCue)
                Debug.Log($"[CueStick][Stats] RegisterShot({p})");
        }

        SetAiming(false);

        Vector3 shootDir = pivot.forward;
        shootDir.y = 0f;
        shootDir.Normalize();

        float impulse = currentPower01 * maxImpulse;
        cueBallRb.AddForce(shootDir * impulse, ForceMode.Impulse);

        float spinStrength = currentPower01;

        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, shootDir).normalized;

        Vector3 sideTorque = up * (spin.x * maxSpinTorque * spinStrength);
        Vector3 topBackTorque = right * (-spin.y * maxSpinTorque * spinStrength);

        cueBallRb.AddTorque(sideTorque + topBackTorque, ForceMode.Impulse);

        if (debugCue)
            Debug.Log($"[CueStick] SHOOT power={currentPower01:F2} impulse={impulse:F2} spin={spin}");

        currentPower01 = 0f;

        if (resetSpinAfterShot)
            spin = Vector2.zero;
    }

    private void HandleAllBallsStopped()
    {
        if (debugCue)
            Debug.Log("<color=cyan>[CueStick] HandleAllBallsStopped()</color>");

        if (IsBallInHand)
            return;

        ShotTracker.Instance?.ResolveShot();

        SetAiming(true);

        currentPower01 = 0f;
        isDraggingPower = false;

        SnapPivotToCueBall();
        ResetVisualToRest();
    }

    private void SetAiming(bool aiming)
    {
        if (isAiming == aiming)
            return;

        isAiming = aiming;
        NotifyCueReady(isAiming);
    }

    private void NotifyCueReady(bool ready)
    {
        if (debugCue)
            Debug.Log($"<color=cyan>[CueStick]</color> CueReady = {ready}");

        OnCueReadyChanged?.Invoke(ready);
    }

    public void EnterBallInHandMode()
    {
        IsBallInHand = true;

        ShotTracker.Instance?.CancelShot();

        SetAiming(false);
        SetCueVisible(false);

        currentPower01 = 0f;
        isDraggingPower = false;

        if (debugCue)
            Debug.Log("<color=yellow>[CueStick]</color> EnterBallInHandMode()");
    }

    public void ExitBallInHandMode()
    {
        IsBallInHand = false;

        SetCueVisible(true);
        SetAiming(true);

        SnapPivotToCueBall();
        ResetVisualToRest();

        if (debugCue)
            Debug.Log("<color=yellow>[CueStick]</color> ExitBallInHandMode()");
    }

    public void SetCueVisible(bool visible)
    {
        if (cueStickVisualRoot != null)
        {
            cueStickVisualRoot.SetActive(visible);
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;
    }

    private void AimAtPointer()
    {
        if (aimCamera == null)
            return;

        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        Vector3? targetOpt = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tableLayerMask))
        {
            Vector3 target = hit.point;
            target.y = cueBallRb.position.y;
            targetOpt = target;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0f, cueBallRb.position.y, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 target = ray.GetPoint(enter);
                target.y = cueBallRb.position.y;
                targetOpt = target;
            }
        }

        if (!targetOpt.HasValue)
            return;

        Vector3 dir = targetOpt.Value - cueBallRb.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        ApplyAimedYaw(dir);
    }

    private void ApplyAimedYaw(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return;

        float desiredYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float currentYaw = pivot.eulerAngles.y;

        float smoothedYaw = Mathf.SmoothDampAngle(
            currentYaw,
            desiredYaw,
            ref _aimYawVel,
            aimYawSmoothTime
        );

        float maxStep = aimRotateSpeedDegPerSec * Time.deltaTime;
        float newYaw = Mathf.MoveTowardsAngle(currentYaw, smoothedYaw, maxStep);

        pivot.rotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    private void SnapPivotToCueBall()
    {
        Vector3 ballPos = cueBallRb.position;
        pivot.position = ballPos;
    }

    private void PlaceCueVisualAtRest()
    {
        if (visualRootTransform == null)
            return;

        Vector3 localPos = visualRestLocalPosition;
        localPos.y = heightOffset;
        localPos.z = -restDistanceFromBall;

        visualRootTransform.localPosition = localPos;
    }

    private void ApplyVisualPullBack()
    {
        if (visualRootTransform == null)
            return;

        Vector3 localPos = visualRootTransform.localPosition;
        localPos.y = heightOffset;
        localPos.z = -restDistanceFromBall - (currentPower01 * maxPullBackDistance);

        visualRootTransform.localPosition = localPos;
    }

    private void ResetVisualToRest()
    {
        if (visualRootTransform == null)
            return;

        Vector3 localPos = visualRestLocalPosition;
        localPos.y = heightOffset;
        localPos.z = -restDistanceFromBall;

        visualRootTransform.localPosition = localPos;
        visualRootTransform.localRotation = visualRestLocalRotation;
        visualRootTransform.localScale = visualRestLocalScale;
    }
}