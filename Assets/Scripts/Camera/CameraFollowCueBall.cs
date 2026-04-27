using UnityEngine;

public class CameraFollowCueBall : MonoBehaviour
{
    [Header("References")]
    public Transform CueBall;
    public Rigidbody CueBallRb;   // optional but enables speed-based zoom
    public ShotState ShotState;

    [Header("Rig Parts")]
    public Transform RigRoot;      // CameraRig
    public Transform FollowTarget; // child used as VCam Follow target (local offset)

    [Header("Follow")]
    public bool FollowWhileBallsMoving = true;
    public float PositionSmoothTime = 0.08f;

    [Header("Base Offset (local)")]
    public float BaseHeight = 2.5f;
    public float BaseDistance = 4.0f;

    [Header("Speed Zoom (optional)")]
    public bool EnableSpeedZoom = true;
    public float MaxExtraDistance = 2.0f;   // how far back it can zoom
    public float SpeedForMaxZoom = 6.0f;    // cue ball speed to reach max zoom
    public float ZoomSmoothTime = 0.12f;

    Vector3 rigVel;
    float currentDistance;
    float distanceVel;

    void Awake()
    {
        if (RigRoot == null) RigRoot = transform;
        currentDistance = BaseDistance;
    }

    void LateUpdate()
    {
        if (!CueBall || !RigRoot) return;

        bool ballsMoving = ShotState != null && ShotState.State == MatchState.BallsMoving;
        if (!FollowWhileBallsMoving && ballsMoving) return;

        // Smoothly move the rig to the cue ball position
        RigRoot.position = Vector3.SmoothDamp(RigRoot.position, CueBall.position, ref rigVel, PositionSmoothTime);

        // Determine desired distance (speed zoom)
        float desiredDistance = BaseDistance;
        if (EnableSpeedZoom && CueBallRb != null)
        {
            float speed = CueBallRb.linearVelocity.magnitude;
            float t = Mathf.Clamp01(speed / Mathf.Max(0.01f, SpeedForMaxZoom));
            desiredDistance = BaseDistance + (MaxExtraDistance * t);
        }

        currentDistance = Mathf.SmoothDamp(currentDistance, desiredDistance, ref distanceVel, ZoomSmoothTime);

        // Keep the follow target at a stable offset in local space
        if (FollowTarget)
            FollowTarget.localPosition = new Vector3(0f, BaseHeight, -currentDistance);
    }
}