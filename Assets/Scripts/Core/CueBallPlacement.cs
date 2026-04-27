// CueBallPlacement.cs
using System;
using System.Collections;
using UnityEngine;

public class CueBallPlacement : MonoBehaviour
{
    [Header("Camera (Single Camera Blend)")]
    public Camera normalCamera;
    public Transform normalCamPose;
    public Transform topDownCamPose;
    public float cameraBlendDuration = 0.6f;
    public AnimationCurve cameraBlendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Disable During Ball-In-Hand")]
    public CinematicPoolCamera cinematicPoolCamera;

    [Header("Placement Plane")]
    public float tableY = 0.75f;

    [Header("Validation")]
    public float overlapCheckRadius = 0.03f;
    public LayerMask ballLayer;

    [Header("Visibility During Placement")]
    public GameObject[] objectsToHideDuringPlacement;

    [Header("Cinematic Restore")]
    [Tooltip("If true, cinematic camera will always be enabled after placement.")]
    public bool forceEnableCinematicAfterPlacement = true;

    [Tooltip("If true, restore PlayerAim regardless of saved view.")]
    public bool forceAimAfterPlacement = true;

    [Tooltip("Prevents autoMode (balls moving) from overriding view right after placement.")]
    public float suppressAutoSecondsAfterPlacement = 0.6f;

    private Rigidbody cueBallRb;
    private Collider cueBallCollider;
    private bool placing;
    private Plane tablePlane;

    private Coroutine camBlendRoutine;
    private CueStickController cueStick;

    private bool _cinematicWasEnabled;
    private bool _savedViewValid;
    private CinematicPoolCamera.ViewMode _savedPlayerView;

    private TurnManager _turnManager;

    private Camera ActiveCamera => normalCamera;

    public event Action OnPlacementConfirmed;
    public bool IsPlacing => placing;

    private void Awake()
    {
        if (normalCamera == null) normalCamera = Camera.main;

        if (normalCamera != null && normalCamPose != null)
        {
            normalCamera.transform.position = normalCamPose.position;
            normalCamera.transform.rotation = normalCamPose.rotation;
        }

        cueStick = CueStickController.Instance;
        if (cueStick == null)
            cueStick = FindFirstObjectByType<CueStickController>();

        if (cinematicPoolCamera == null)
            cinematicPoolCamera = FindFirstObjectByType<CinematicPoolCamera>();

        if (_turnManager == null)
            _turnManager = FindFirstObjectByType<TurnManager>();
    }

    private void Update()
    {
        if (!placing || cueBallRb == null)
            return;

        MoveWithMouse();

        if (Input.GetMouseButtonDown(0))
            TryConfirmPlacement();
    }

    public void BeginPlacement(Rigidbody cueBall)
    {
        if (cueBall == null)
        {
            Debug.LogWarning("[Ball In Hand] BeginPlacement called with null cueBall.");
            return;
        }

        cueBallRb = cueBall;
        cueBallCollider = cueBallRb.GetComponent<Collider>();
        placing = true;

        if (cinematicPoolCamera != null)
        {
            _cinematicWasEnabled = cinematicPoolCamera.enabled;

            _savedViewValid = _cinematicWasEnabled;
            if (_savedViewValid)
                _savedPlayerView = cinematicPoolCamera.GetPreferredRestoreView();

            Debug.Log($"[Ball In Hand] SAVED restoreView={_savedPlayerView} | current={cinematicPoolCamera.GetView()} | lastUser={cinematicPoolCamera.lastUserSelectedViewMode} | hasUserSelected={cinematicPoolCamera.hasUserSelectedView}");

            // Disable cinematic so it cannot fight our manual blend camera during placement
            cinematicPoolCamera.enabled = false;
        }
        else
        {
            _cinematicWasEnabled = false;
            _savedViewValid = false;
        }

        SetPlacementVisibility(true);
        cueStick?.EnterBallInHandMode();

        BlendCameraTo(topDownCamPose);

        tablePlane = new Plane(Vector3.up, new Vector3(0f, tableY, 0f));

        cueBallRb.linearVelocity = Vector3.zero;
        cueBallRb.angularVelocity = Vector3.zero;
        cueBallRb.isKinematic = true;
        cueBallRb.detectCollisions = false;

        if (cueBallCollider != null)
            cueBallCollider.enabled = false;

        Debug.Log("<color=yellow>[Ball In Hand]</color> Placement started (camera blending to top-down).");
    }

    private void MoveWithMouse()
    {
        Camera cam = ActiveCamera;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (tablePlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            hitPoint.y = tableY;

            cueBallRb.position = hitPoint;
        }
    }

    private void TryConfirmPlacement()
    {
        if (IsOverlapping())
        {
            Debug.Log("<color=red>[Ball In Hand]</color> Invalid placement: overlapping another ball.");
            return;
        }

        EndPlacement(true);
    }

    private void EndPlacement(bool confirmed)
    {
        placing = false;

        SetPlacementVisibility(false);
        cueStick?.ExitBallInHandMode();

        // Cached ref to avoid runtime searching on click
        _turnManager?.ConfirmBallInHandPlaced();

        if (cueBallCollider != null)
            cueBallCollider.enabled = true;

        cueBallRb.detectCollisions = true;
        cueBallRb.isKinematic = false;

        cueBallRb.linearVelocity = Vector3.zero;
        cueBallRb.angularVelocity = Vector3.zero;

        if (normalCamera == null || normalCamPose == null)
        {
            Debug.LogWarning("[Ball In Hand] normalCamera or normalCamPose is NULL. Skipping blend; restoring cinematic immediately.");
            RestoreCinematicClean();
        }
        else
        {
            BlendCameraTo(normalCamPose, RestoreCinematicClean);
        }

        if (confirmed)
            OnPlacementConfirmed?.Invoke();

        Debug.Log(confirmed
            ? "<color=green>[Ball In Hand]</color> Placement confirmed (camera blending to normal)."
            : "<color=orange>[Ball In Hand]</color> Placement ended (camera blending to normal).");

        cueBallRb = null;
        cueBallCollider = null;
    }

    /// <summary>
    /// Clean restore: enable cinematic once, set intended view once, suppress auto to avoid snap-back to BroadcastCorner.
    /// </summary>
    private void RestoreCinematicClean()
    {
        if (cinematicPoolCamera == null) return;

        bool desiredEnabled = forceEnableCinematicAfterPlacement ? true : _cinematicWasEnabled;

        // Decide what view we WANT after placement
        CinematicPoolCamera.ViewMode target =
            forceAimAfterPlacement ? CinematicPoolCamera.ViewMode.PlayerAim :
            (_savedViewValid ? _savedPlayerView : CinematicPoolCamera.ViewMode.PlayerAim);

        // Apply while disabled so it doesn't run update in between steps
        cinematicPoolCamera.enabled = false;

        // lockAuto=true ensures manual override inside CinematicPoolCamera
        cinematicPoolCamera.SetView((int)target, true);

        cinematicPoolCamera.enabled = desiredEnabled;

        // Prevent autoMode "balls moving" from overriding right after restore
        cinematicPoolCamera.SuppressAutoFor(suppressAutoSecondsAfterPlacement);

        Debug.Log($"[Ball In Hand] Cinematic RESTORE: enabled={cinematicPoolCamera.enabled} target={target} viewNow={cinematicPoolCamera.GetView()} suppressAuto={suppressAutoSecondsAfterPlacement:0.00}s");
    }

    private void SetPlacementVisibility(bool placingState)
    {
        if (objectsToHideDuringPlacement == null) return;

        foreach (var obj in objectsToHideDuringPlacement)
        {
            if (obj != null)
                obj.SetActive(!placingState);
        }
    }

    private bool IsOverlapping()
    {
        if (cueBallRb == null) return false;

        Collider[] hits = Physics.OverlapSphere(cueBallRb.position, overlapCheckRadius, ballLayer);

        foreach (var hit in hits)
        {
            if (hit != null && hit.attachedRigidbody != null && hit.attachedRigidbody != cueBallRb)
                return true;
        }

        return false;
    }

    private void BlendCameraTo(Transform targetPose) => BlendCameraTo(targetPose, null);

    private void BlendCameraTo(Transform targetPose, Action onComplete)
    {
        if (normalCamera == null || targetPose == null)
        {
            Debug.LogWarning("[Ball In Hand] BlendCameraTo skipped (normalCamera/targetPose null).");
            onComplete?.Invoke();
            return;
        }

        if (camBlendRoutine != null)
            StopCoroutine(camBlendRoutine);

        camBlendRoutine = StartCoroutine(BlendRoutine(targetPose, onComplete));
    }

    private IEnumerator BlendRoutine(Transform targetPose, Action onComplete)
    {
        Transform camT = normalCamera.transform;

        Vector3 startPos = camT.position;
        Quaternion startRot = camT.rotation;

        Vector3 endPos = targetPose.position;
        Quaternion endRot = targetPose.rotation;

        float t = 0f;
        float d = Mathf.Max(0.01f, cameraBlendDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / d;
            float eased = cameraBlendCurve.Evaluate(Mathf.Clamp01(t));

            camT.position = Vector3.Lerp(startPos, endPos, eased);
            camT.rotation = Quaternion.Slerp(startRot, endRot, eased);

            yield return null;
        }

        camT.position = endPos;
        camT.rotation = endRot;

        camBlendRoutine = null;

        onComplete?.Invoke();
    }
}