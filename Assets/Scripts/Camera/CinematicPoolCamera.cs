// CinematicPoolCamera.cs
using UnityEngine;

public class CinematicPoolCamera : MonoBehaviour
{
    public enum ViewMode { BroadcastCorner, PlayerAim, TopDown }

    [Header("Targets")]
    public Transform tableRoot;          // Center of table
    public Transform cueBall;            // Cue ball transform
    public Transform aimTargetOptional;  // Optional: raycast hit / ghost ball / aim point

    [Header("Cue Stick (for Aim camera follow)")]
    [Tooltip("Assign CueStickController OR directly assign cueStickPivot.")]
    public CueStickController cueStickController;
    [Tooltip("If CueStickController is assigned, this will auto-fill. Otherwise assign pivot directly.")]
    public Transform cueStickPivot;

    [Header("Balls Stopped Detector (optional but recommended)")]
    public BallsStoppedDetector detector;
    [Tooltip("If true: auto-switch views when balls move/stop.")]
    public bool autoMode = true;
    public ViewMode viewWhileBallsMoving = ViewMode.BroadcastCorner;
    public ViewMode viewWhenBallsStopped = ViewMode.PlayerAim;

    [Header("Debug")]
    [Tooltip("Shows the current camera view mode (read only).")]
    public ViewMode currentViewMode;

    [Tooltip("Tracks the last view the PLAYER selected (not auto-switched).")]
    public ViewMode lastUserSelectedViewMode;

    [Tooltip("True once the player has manually selected a view (C key / UI).")]
    public bool hasUserSelectedView;

    [Header("Switching")]
    public ViewMode startView = ViewMode.BroadcastCorner;
    public bool allowKeyboardSwitch = true;
    public KeyCode switchKey = KeyCode.C;

    [Header("Smoothing")]
    [Range(0.01f, 30f)] public float positionSmooth = 10f;
    [Range(0.01f, 30f)] public float rotationSmooth = 12f;

    [Header("Cinematic Motion (minor noise only, not orbit)")]
    public bool cinematicMotion = true;
    [Range(0f, 0.2f)] public float bobAmount = 0.02f;
    [Range(0f, 0.5f)] public float noiseAmount = 0.06f;
    public float noiseSpeed = 0.25f;

    [Header("Ball In Hand (TopDown Placement Controls)")]
    public bool enableBallInHandPan = true;
    public float panSpeed = 1.2f;
    public float panMouseSensitivity = 0.0022f;

    [Header("View 1: Broadcast (MMB Orbit + Zoom)")]
    public float broadcastFov = 55f;
    [Tooltip("Hold Middle Mouse Button and drag to rotate.")]
    public float mouseSensitivity = 2.2f;
    [Tooltip("Inertia: higher = stops sooner, lower = glides longer.")]
    public float inertiaDamping = 10f;
    [Tooltip("Max degrees/sec the inertia can carry after release.")]
    public float maxInertiaSpeed = 260f;
    [Tooltip("Min/max vertical angle for orbit.")]
    public float minVerticalAngle = 12f;
    public float maxVerticalAngle = 75f;
    [Tooltip("Zoom distance range for Broadcast orbit.")]
    public float minDistance = 1.6f;
    public float maxDistance = 3.6f;
    [Tooltip("Scroll wheel zoom speed.")]
    public float zoomSpeed = 1.0f;
    [Tooltip("How quickly distance smooths to target.")]
    public float zoomSmooth = 14f;
    [Tooltip("Default starting orbit angles.")]
    public float startYaw = 45f;
    public float startPitch = 32f;
    public float startDistance = 2.6f;

    [Header("Auto Focus on Cue Ball (when shot starts)")]
    public bool autoFocusOnShot = true;
    public float focusBlendTime = 0.35f;
    public bool focusOppositeCueBall = true;

    [Header("Broadcast Pivot Behavior")]
    [Tooltip("If true, broadcast pivots around cue ball while balls are moving.")]
    public bool pivotOnCueBallWhileMoving = true;

    [Header("View 2: Player Aim (FOLLOW CUE)")]
    [Tooltip("If true, Aim view yaw follows cueStickPivot yaw.")]
    public bool followCueYawInAim = true;
    [Tooltip("Adds an offset to the follow yaw (often 0).")]
    public float aimYawOffset = 0f;
    [Tooltip("Aim camera distance (behind cue direction).")]
    public float aimDistance = 0.85f;
    [Tooltip("Aim camera height above ball.")]
    public float aimHeight = 0.22f;
    [Tooltip("Side offset (left/right) in Aim view.")]
    public float aimSideOffset = 0f;
    [Tooltip("If true, MMB can add extra yaw while aiming (fine adjustment).")]
    public bool allowMMBAdjustInAim = false;
    [Tooltip("How quickly the camera yaw snaps to cue yaw in Aim view.")]
    public float aimYawFollowSmooth = 18f;
    public float aimFov = 45f;

    [Header("View 3: Top Down")]
    public float topDownHeight = 2.8f;
    public float topDownTilt = 85f;
    public float topDownFov = 60f;

    [Header("DEBUG (optional)")]
    public bool debugMMB = false;

    // --- internals ---
    private Camera _cam;
    private ViewMode _mode;

    private Vector3 _posVel;
    private float _seed;

    private float _yaw;
    private float _pitch;
    private float _yawVel;
    private float _pitchVel;

    private float _distance;
    private float _targetDistance;

    private bool _wasStoppedLastFrame = true;

    private bool _focusing;
    private float _focusT;
    private float _focusStartYaw, _focusStartPitch;
    private float _focusTargetYaw, _focusTargetPitch;

    private Vector3 _lastMousePos;
    private bool _hasLastMousePos;

    private float _aimYawAdd;

    private Vector3 _topDownPanOffset;
    private bool _ballInHandActive;
    private ViewMode _prevMode;
    private bool _prevAutoMode;
    private bool _prevAllowKeyboardSwitch;

    private Vector3 _panLastMouse;
    private bool _panning;

    // prevents autoMode from overriding player's choice (used for restore too)
    private bool _manualOverride;

    // NEW: external lock + suppression window (stops "balls moving" snapping after BIH)
    private bool _externalAutoLock;
    private float _suppressAutoUntil;

    /// <summary>
    /// Hard lock: when true, auto switching will not change the view at all.
    /// </summary>
    public void SetExternalAutoLock(bool locked) => _externalAutoLock = locked;

    /// <summary>
    /// Soft lock: temporarily suppress auto switching for a short time.
    /// Great for BIH restore / post-shot transitions.
    /// </summary>
    public void SuppressAutoFor(float seconds)
    {
        _suppressAutoUntil = Mathf.Max(_suppressAutoUntil, Time.time + Mathf.Max(0f, seconds));
    }

    private bool IsAutoSuppressed => Time.time < _suppressAutoUntil;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;

        _mode = startView;

        lastUserSelectedViewMode = startView;
        hasUserSelectedView = false;

        _seed = Random.Range(0f, 1000f);

        _yaw = startYaw;
        _pitch = Mathf.Clamp(startPitch, minVerticalAngle, maxVerticalAngle);
        _distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        _targetDistance = _distance;
    }

    void OnEnable()
    {
        if (detector != null)
            detector.OnAllBallsStopped += HandleAllBallsStopped;
    }

    void OnDisable()
    {
        if (detector != null)
            detector.OnAllBallsStopped -= HandleAllBallsStopped;
    }

    void Start()
    {
        if (detector == null)
            detector = FindFirstObjectByType<BallsStoppedDetector>();

        if (cueStickController == null)
            cueStickController = FindFirstObjectByType<CueStickController>();

        if (cueStickPivot == null && cueStickController != null)
            cueStickPivot = cueStickController.cueStickPivot != null ? cueStickController.cueStickPivot : cueStickController.transform;

        if (detector != null)
        {
            detector.OnAllBallsStopped -= HandleAllBallsStopped;
            detector.OnAllBallsStopped += HandleAllBallsStopped;

            _wasStoppedLastFrame = detector.AreBallsStopped;

            if (autoMode && detector.AreBallsStopped && !_manualOverride && !_externalAutoLock)
            {
                _mode = viewWhenBallsStopped;
                if (!hasUserSelectedView)
                    lastUserSelectedViewMode = _mode;
            }
        }

        Debug.Log(
            $"[CinematicPoolCamera] START VIEW = {_mode} | " +
            $"AutoMode={autoMode} | " +
            $"StartViewSetting={startView} | " +
            $"BallsStopped={(detector != null ? detector.AreBallsStopped : false)} | " +
            $"lastUserSelected={lastUserSelectedViewMode} hasUserSelected={hasUserSelectedView}"
        );
    }

    void Update()
    {
        if (allowKeyboardSwitch && Input.GetKeyDown(switchKey))
            NextView();

        // Auto switching only if allowed
        bool canAuto =
            autoMode &&
            detector != null &&
            !_manualOverride &&
            !_externalAutoLock &&
            !IsAutoSuppressed;

        if (canAuto)
        {
            bool stopped = detector.AreBallsStopped;

            // Transition: stopped -> moving (shot started)
            if (_wasStoppedLastFrame && !stopped)
            {
                _mode = viewWhileBallsMoving;

                if (autoFocusOnShot && _mode == ViewMode.BroadcastCorner)
                    BeginFocusToCueBall();
            }

            _wasStoppedLastFrame = stopped;
        }
        else if (detector != null)
        {
            _wasStoppedLastFrame = detector.AreBallsStopped;
        }

        // Clear manual override when balls start moving again (optional)
        if (autoMode && detector != null && _manualOverride)
        {
            if (!detector.AreBallsStopped)
                _manualOverride = false;
        }

        if (debugMMB && Input.GetMouseButtonDown(2))
            Debug.Log("[CinematicPoolCamera] MMB DOWN detected.");

        currentViewMode = _mode;
    }

    void LateUpdate()
    {
        if (!tableRoot || !cueBall) return;

        Vector3 desiredPos = transform.position;
        Quaternion desiredRot = transform.rotation;
        float desiredFov = (_cam != null) ? _cam.fieldOfView : 60f;

        Vector3 aimPoint = (aimTargetOptional != null)
            ? aimTargetOptional.position
            : (cueBall.position + cueBall.forward * 1.0f);

        float t = Time.time;

        if (_mode == ViewMode.BroadcastCorner)
        {
            HandleBroadcastOrbitInput();
            HandleBroadcastZoomInput();

            _distance = Mathf.Lerp(_distance, _targetDistance, 1f - Mathf.Exp(-zoomSmooth * Time.deltaTime));

            if (_focusing)
            {
                _focusT += Time.deltaTime / Mathf.Max(0.01f, focusBlendTime);
                float k = Smooth01(Mathf.Clamp01(_focusT));

                _yaw = Mathf.LerpAngle(_focusStartYaw, _focusTargetYaw, k);
                _pitch = Mathf.Lerp(_focusStartPitch, _focusTargetPitch, k);

                if (_focusT >= 1f) _focusing = false;
            }

            Vector3 pivot = tableRoot.position;
            if (pivotOnCueBallWhileMoving && detector != null && !detector.AreBallsStopped)
                pivot = cueBall.position;

            Quaternion orbitRot = Quaternion.Euler(_pitch, _yaw, 0f);
            desiredPos = pivot + (orbitRot * new Vector3(0f, 0f, -_distance));

            Vector3 lookPoint = Vector3.Lerp(tableRoot.position, cueBall.position, 0.65f) + Vector3.up * 0.06f;
            desiredPos += CinematicNoise(t) * 0.35f;

            desiredRot = Quaternion.LookRotation((lookPoint - desiredPos).normalized, Vector3.up);
            desiredFov = broadcastFov;
        }
        else if (_mode == ViewMode.PlayerAim)
        {
            Vector3 dir;

            if (followCueYawInAim && cueStickPivot != null)
            {
                dir = cueStickPivot.forward;
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) dir = cueBall.forward;
                dir = dir.normalized;

                float targetYaw = cueStickPivot.eulerAngles.y + aimYawOffset + _aimYawAdd;
                _yaw = Mathf.LerpAngle(_yaw, targetYaw, 1f - Mathf.Exp(-aimYawFollowSmooth * Time.deltaTime));
            }
            else
            {
                dir = (aimPoint - cueBall.position);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) dir = cueBall.forward;
                dir = dir.normalized;

                float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + _aimYawAdd;
                _yaw = Mathf.LerpAngle(_yaw, targetYaw, 1f - Mathf.Exp(-aimYawFollowSmooth * Time.deltaTime));
            }

            if (allowMMBAdjustInAim)
                HandleAimMMBAddYaw();

            Vector3 yawDir = Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;

            Vector3 camPos = cueBall.position - yawDir * aimDistance;
            camPos += Vector3.up * aimHeight;
            camPos += Vector3.Cross(Vector3.up, yawDir).normalized * aimSideOffset;

            desiredPos = camPos;

            Vector3 lookPoint = cueBall.position + yawDir * 0.8f + Vector3.up * 0.03f;
            desiredPos += CinematicNoise(t) * 0.18f;

            desiredRot = Quaternion.LookRotation((lookPoint - desiredPos).normalized, Vector3.up);
            desiredFov = aimFov;
        }
        else // TopDown
        {
            Vector3 center = Vector3.Lerp(tableRoot.position, cueBall.position, 0.25f);

            if (_ballInHandActive)
                HandleTopDownPan();

            center += _topDownPanOffset;

            desiredPos = center + Vector3.up * topDownHeight;
            desiredRot = Quaternion.Euler(topDownTilt, 0f, 0f);
            desiredFov = topDownFov;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _posVel,
            1f / Mathf.Max(0.001f, positionSmooth)
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRot,
            1f - Mathf.Exp(-rotationSmooth * Time.deltaTime)
        );

        if (_cam != null)
            _cam.fieldOfView = Mathf.Lerp(
                _cam.fieldOfView,
                desiredFov,
                1f - Mathf.Exp(-8f * Time.deltaTime)
            );
    }

    private void HandleBroadcastOrbitInput()
    {
        bool holdingMMB = Input.GetMouseButton(2);

        if (holdingMMB)
        {
            Vector3 current = Input.mousePosition;

            if (!_hasLastMousePos)
            {
                _lastMousePos = current;
                _hasLastMousePos = true;
                return;
            }

            Vector3 delta = current - _lastMousePos;
            _lastMousePos = current;

            float yawDelta = delta.x * mouseSensitivity * 0.12f;
            float pitchDelta = -delta.y * mouseSensitivity * 0.12f;

            _yaw += yawDelta;
            _pitch += pitchDelta;
            _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);

            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            _yawVel = Mathf.Clamp(yawDelta / dt, -maxInertiaSpeed, maxInertiaSpeed);
            _pitchVel = Mathf.Clamp(pitchDelta / dt, -maxInertiaSpeed, maxInertiaSpeed);

            _focusing = false;
        }
        else
        {
            _hasLastMousePos = false;

            if (Mathf.Abs(_yawVel) > 0.01f || Mathf.Abs(_pitchVel) > 0.01f)
            {
                _yaw += _yawVel * Time.deltaTime;
                _pitch += _pitchVel * Time.deltaTime;
                _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);

                float damp = 1f - Mathf.Exp(-inertiaDamping * Time.deltaTime);
                _yawVel = Mathf.Lerp(_yawVel, 0f, damp);
                _pitchVel = Mathf.Lerp(_pitchVel, 0f, damp);
            }
        }
    }

    private void HandleAimMMBAddYaw()
    {
        bool holdingMMB = Input.GetMouseButton(2);

        if (holdingMMB)
        {
            Vector3 current = Input.mousePosition;

            if (!_hasLastMousePos)
            {
                _lastMousePos = current;
                _hasLastMousePos = true;
                return;
            }

            Vector3 delta = current - _lastMousePos;
            _lastMousePos = current;

            float yawDelta = delta.x * mouseSensitivity * 0.10f;
            _aimYawAdd += yawDelta;
        }
        else
        {
            _hasLastMousePos = false;
        }
    }

    private void HandleBroadcastZoomInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.001f) return;

        _targetDistance -= scroll * zoomSpeed * 0.25f;
        _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
    }

    public void EnterBallInHandView()
    {
        _ballInHandActive = true;

        _prevMode = _mode;
        _prevAutoMode = autoMode;
        _prevAllowKeyboardSwitch = allowKeyboardSwitch;

        autoMode = false;
        allowKeyboardSwitch = false;

        _mode = ViewMode.TopDown;

        _yawVel = 0f;
        _pitchVel = 0f;
        _focusing = false;
    }

    public void ExitBallInHandView()
    {
        _ballInHandActive = false;

        autoMode = _prevAutoMode;
        allowKeyboardSwitch = _prevAllowKeyboardSwitch;

        _mode = _prevMode;

        _yawVel = 0f;
        _pitchVel = 0f;
        _focusing = false;

        _panning = false;
    }

    private void BeginFocusToCueBall()
    {
        if (!tableRoot || !cueBall) return;

        Vector3 toCue = cueBall.position - tableRoot.position;
        toCue.y = 0f;
        if (toCue.sqrMagnitude < 0.0001f) return;

        float cueYaw = Mathf.Atan2(toCue.x, toCue.z) * Mathf.Rad2Deg;

        float targetYaw = focusOppositeCueBall ? cueYaw + 180f : cueYaw;
        float targetPitch = Mathf.Clamp(_pitch, 22f, 45f);

        _focusStartYaw = _yaw;
        _focusStartPitch = _pitch;
        _focusTargetYaw = targetYaw;
        _focusTargetPitch = targetPitch;

        _focusT = 0f;
        _focusing = true;
    }

    private Vector3 CinematicNoise(float t)
    {
        if (!cinematicMotion) return Vector3.zero;

        float n1 = Mathf.PerlinNoise(_seed, t * noiseSpeed) - 0.5f;
        float n2 = Mathf.PerlinNoise(_seed + 10f, t * noiseSpeed) - 0.5f;

        Vector3 drift = new Vector3(n1, 0f, n2) * noiseAmount;
        Vector3 bob = Vector3.up * (Mathf.Sin(t * 1.2f) * bobAmount);

        return drift + bob;
    }

    private static float Smooth01(float x) => x * x * (3f - 2f * x);

    private void HandleTopDownPan()
    {
        if (!enableBallInHandPan) return;

        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;

        Vector3 keyboardMove = (Vector3.right * h + Vector3.forward * v) * (panSpeed * Time.deltaTime);

        if (Input.GetMouseButtonDown(1))
        {
            _panning = true;
            _panLastMouse = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
            _panning = false;

        Vector3 mouseMove = Vector3.zero;
        if (_panning)
        {
            Vector3 cur = Input.mousePosition;
            Vector3 delta = cur - _panLastMouse;
            _panLastMouse = cur;

            mouseMove = new Vector3(-delta.x, 0f, -delta.y) * panMouseSensitivity;
        }

        _topDownPanOffset += keyboardMove + mouseMove;
    }

    // ---------- UI ----------
    public void NextView()
    {
        _mode = (ViewMode)(((int)_mode + 1) % 3);

        lastUserSelectedViewMode = _mode;
        hasUserSelectedView = true;

        _manualOverride = true;

        _focusing = false;
        _yawVel = 0f;
        _pitchVel = 0f;
    }

    public void SetView(int modeIndex, bool lockAuto = true)
    {
        modeIndex = Mathf.Clamp(modeIndex, 0, 2);
        _mode = (ViewMode)modeIndex;

        if (lockAuto)
        {
            lastUserSelectedViewMode = _mode;
            hasUserSelectedView = true;
            _manualOverride = true;
        }

        _focusing = false;
        _yawVel = 0f;
        _pitchVel = 0f;
    }

    public void SetView(int modeIndex) => SetView(modeIndex, true);

    public ViewMode GetView() => _mode;

    public ViewMode GetLastUserView() => lastUserSelectedViewMode;

    public ViewMode GetPreferredRestoreView()
    {
        return hasUserSelectedView ? lastUserSelectedViewMode : _mode;
    }

    private void HandleAllBallsStopped()
    {
        // Don't let stopped-event override if we're locked/suppressed/manual
        if (!autoMode || _manualOverride || _externalAutoLock || IsAutoSuppressed) return;

        _mode = viewWhenBallsStopped;

        _yawVel = 0f;
        _pitchVel = 0f;
        _focusing = false;
        _aimYawAdd = 0f;
    }
}