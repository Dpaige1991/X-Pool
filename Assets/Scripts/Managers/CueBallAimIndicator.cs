using UnityEngine;

public class CueBallAimIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cueBall;
    [SerializeField] private Transform aimOrigin;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject hitMarker;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private bool showOnlyWhenAiming = true;
    [SerializeField] private bool isAiming = true;
    [SerializeField] private float startYOffset = 0.03f;
    [SerializeField] private float markerYOffset = 0.1f;
    [SerializeField] private bool scaleMarkerByDistance = true;
    [SerializeField] private float markerScaleMultiplier = 0.05f;
    [SerializeField] private float minMarkerScale = 0.2f;
    [SerializeField] private float maxMarkerScale = 0.6f;

    private CueStickController cueController;

    private void Reset()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        cueController = CueStickController.Instance;

        if (cueController != null)
        {
            cueController.OnCueReadyChanged += HandleCueReadyChanged;
            SetAiming(cueController.IsCueReady);
        }
        else
        {
            SetAiming(isAiming);
        }

        if (!isAiming)
            HideIndicator();
    }

    private void Update()
    {
        if (cueBall == null || aimOrigin == null || lineRenderer == null)
            return;

        if (showOnlyWhenAiming && !isAiming)
        {
            HideIndicator();
            return;
        }

        ShowAimIndicator();
    }

    private void OnDestroy()
    {
        if (cueController != null)
            cueController.OnCueReadyChanged -= HandleCueReadyChanged;
    }

    private void HandleCueReadyChanged(bool ready)
    {
        SetAiming(ready);
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;

        if (!isAiming)
            HideIndicator();
    }

    private void ShowAimIndicator()
    {
        Vector3 start = cueBall.position + Vector3.up * startYOffset;
        Vector3 direction = aimOrigin.forward.normalized;

        Ray ray = new Ray(start, direction);

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;

        Vector3 hitPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, collisionMask))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = start + direction * maxDistance;
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, hitPoint);

        if (hitMarker != null)
        {
            hitMarker.SetActive(true);
            hitMarker.transform.position = hitPoint + Vector3.up * markerYOffset;

            if (scaleMarkerByDistance)
            {
                float distance = Vector3.Distance(start, hitPoint);
                float scale = Mathf.Clamp(
                    distance * markerScaleMultiplier,
                    minMarkerScale,
                    maxMarkerScale
                );

                hitMarker.transform.localScale = Vector3.one * scale;
            }
        }
    }

    private void HideIndicator()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (hitMarker != null)
            hitMarker.SetActive(false);
    }
}