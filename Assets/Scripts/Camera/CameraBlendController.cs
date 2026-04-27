using UnityEngine;
using System.Collections;

public class CameraBlendController : MonoBehaviour
{
    [Header("What moves")]
    public Transform cameraToMove;          // usually your Main Camera transform

    [Header("Poses")]
    public Transform normalPose;            // empty transform at normal camera pose
    public Transform topDownPose;           // empty transform at top-down pose

    [Header("Blend")]
    public float blendDuration = 0.45f;
    public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _blendRoutine;

    private void Awake()
    {
        if (cameraToMove == null) cameraToMove = Camera.main != null ? Camera.main.transform : null;

        // Optional: snap to normal at start
        if (cameraToMove != null && normalPose != null)
        {
            cameraToMove.position = normalPose.position;
            cameraToMove.rotation = normalPose.rotation;
        }
    }

    public void BlendToTopDown() => BlendTo(topDownPose);
    public void BlendToNormal() => BlendTo(normalPose);

    public void BlendTo(Transform targetPose)
    {
        if (cameraToMove == null || targetPose == null)
        {
            Debug.LogWarning("[CameraBlendController] Missing references.");
            return;
        }

        if (_blendRoutine != null) StopCoroutine(_blendRoutine);
        _blendRoutine = StartCoroutine(BlendRoutine(targetPose));
    }

    private IEnumerator BlendRoutine(Transform targetPose)
    {
        Vector3 startPos = cameraToMove.position;
        Quaternion startRot = cameraToMove.rotation;

        Vector3 endPos = targetPose.position;
        Quaternion endRot = targetPose.rotation;

        float t = 0f;
        float d = Mathf.Max(0.01f, blendDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / d;
            float eased = blendCurve.Evaluate(Mathf.Clamp01(t));

            cameraToMove.position = Vector3.Lerp(startPos, endPos, eased);
            cameraToMove.rotation = Quaternion.Slerp(startRot, endRot, eased);

            yield return null;
        }

        cameraToMove.position = endPos;
        cameraToMove.rotation = endRot;

        _blendRoutine = null;
    }
}