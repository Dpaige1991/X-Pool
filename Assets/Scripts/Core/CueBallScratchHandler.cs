using UnityEngine;
using TMPro;
using System.Collections;

public class CueBallScratchHandler : MonoBehaviour
{
    public static CueBallScratchHandler Instance { get; private set; }

    [Header("Scratch Parking")]
    public Transform scratchParkPoint;

    [Header("UI")]
    public GameObject inGameTextObject;
    public TMP_Text groupAssignmentText;
    public float messageDuration = 2f;

    private CueBallPlacement placer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inGameTextObject != null)
            inGameTextObject.SetActive(false);

        placer = FindObjectOfType<CueBallPlacement>();
    }

    public void HandleScratch(Ball cueBall)
    {
        if (cueBall == null)
        {
            Debug.LogWarning("[Scratch] cueBall was null.");
            return;
        }

        if (scratchParkPoint == null)
        {
            Debug.LogWarning("[Scratch] scratchParkPoint not assigned!");
            return;
        }

        // IMPORTANT: Rigidbody might not be on the same GameObject as Ball
        Rigidbody rb =
            cueBall.GetComponent<Rigidbody>() ??
            cueBall.GetComponentInChildren<Rigidbody>() ??
            cueBall.GetComponentInParent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning($"[Scratch] No Rigidbody found on Ball '{cueBall.name}' (or its parent/children).");
            return;
        }

        Debug.Log($"<color=red>[Scratch]</color> Before move: RB pos={rb.position}, Transform pos={rb.transform.position}, Park pos={scratchParkPoint.position}");

        // Make sure we can clear velocity (must be non-kinematic)
        if (rb.isKinematic)
            rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Disable collisions during teleport to avoid weird contact resolution
        rb.detectCollisions = false;

        // Move BOTH the Rigidbody and its Transform (covers setups where visuals are not where RB is)
        rb.position = scratchParkPoint.position;
        rb.rotation = scratchParkPoint.rotation;

        rb.transform.position = scratchParkPoint.position;
        rb.transform.rotation = scratchParkPoint.rotation;

        // Force Unity to apply the transform changes immediately
        Physics.SyncTransforms();

        // Freeze for ball-in-hand placement
        rb.isKinematic = true;

        rb.detectCollisions = true;

        if (placer != null)
        {
            placer.BeginPlacement(rb);
        }

        FindObjectOfType<CueBallPlacement>()?.BeginPlacement(rb);

        Debug.Log($"<color=red>[Scratch]</color> After move: RB pos={rb.position}, Transform pos={rb.transform.position}");

        ShowScratchMessage();
    }

    private void ShowScratchMessage()
    {
        if (inGameTextObject == null || groupAssignmentText == null)
            return;

        groupAssignmentText.text = "Pocketed Cue Ball";

        StopAllCoroutines();
        StartCoroutine(ShowMessageRoutine());
    }

    private IEnumerator ShowMessageRoutine()
    {
        inGameTextObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        inGameTextObject.SetActive(false);
    }
}