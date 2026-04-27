using UnityEngine;

public class CueBallVisibility : MonoBehaviour
{
    public Camera mainCam;
    public Transform cueBall;
    public LayerMask occlusionMask;
    public bool requireNoOcclusion = true;

    public bool IsVisible()
    {
        if (!mainCam || !cueBall) return false;

        Vector3 vp = mainCam.WorldToViewportPoint(cueBall.position);
        bool inFront = vp.z > 0f;
        bool inBounds = vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
        if (!(inFront && inBounds)) return false;

        if (!requireNoOcclusion) return true;

        Vector3 dir = cueBall.position - mainCam.transform.position;
        float dist = dir.magnitude;
        if (Physics.Raycast(mainCam.transform.position, dir.normalized, out var hit, dist, occlusionMask))
        {
            // If we hit something before the cue ball, it's occluded
            if (hit.transform != cueBall) return false;
        }
        return true;
    }
}