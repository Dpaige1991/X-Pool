using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour
{
    public GameObject cueStickPivot;
    public GameObject imaginationBall;
    public LineRenderer aimDirectionLine;
    public LineRenderer cueBallMoveDirectionLine;
    public LineRenderer targetBallMoveDirectionLine;
    public float lineLength = 1.5f, cueBallRadius = 0.1f, whiteEmission, redEmission;

    public static bool lineIsDisplaying = true;

    public string closestBallTag;

    private List<Circle> dangerousCircles = new List<Circle>();
    public List<GameObject> ballObjects = new List<GameObject>();

    TwoPlayerPocket twoPlayerPocketScript;
    OldGameManager gameManagerScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        twoPlayerPocketScript = FindFirstObjectByType<TwoPlayerPocket>();
        gameManagerScript = FindFirstObjectByType<OldGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAimLine();
        DetectDangerousCircles();
    }

    private void UpdateAimLine()
    {
        if (lineIsDisplaying)
        {
            if (gameManagerScript.lineTurnOn)
            {
                cueBallMoveDirectionLine.enabled = true;
                targetBallMoveDirectionLine.enabled = true;
            }
            else
            {
                cueBallMoveDirectionLine.enabled = false;
                targetBallMoveDirectionLine.enabled = false;
            }
            
            imaginationBall.SetActive(true);
            aimDirectionLine.positionCount = 2;

            Vector3 cueBallPos = cueStickPivot.transform.position;
            Vector3 aimDirection = cueStickPivot.transform.forward;

            aimDirectionLine.SetPosition(0, cueBallPos);
            aimDirectionLine.SetPosition(1, cueBallPos + aimDirection * 10f);

            HandleCircleInteractions(cueBallPos, aimDirection);
        }
        else
        {

        }
    }

    private void UpdateAimVisualizeComponent(Vector3 hitPosition, Vector3 targetBallPosition, Vector2 cueBallDirection, Vector2 targetBallDirection)
    {
        Vector2 aimDirection2D = To2D(cueStickPivot.transform.forward);

        float cueBallAligment = Mathf.Abs(Vector2.Dot(cueBallDirection.normalized, aimDirection2D));
        float targetBallAligment = Mathf.Abs(Vector2.Dot(targetBallDirection.normalized, aimDirection2D));

        float totalAligment = cueBallAligment + targetBallAligment;
        cueBallAligment /= totalAligment;
        targetBallAligment /= totalAligment;

        float cueBallLineLength = lineLength * cueBallAligment;
        float targetBallLineLength = lineLength * targetBallAligment;

        LineRenderer lr = cueBallMoveDirectionLine;
        lr.positionCount = 2;
        lr.SetPosition(0, hitPosition);
        lr.SetPosition(1, hitPosition + new Vector3(cueBallDirection.x, 0, cueBallDirection.y) * cueBallLineLength);

        lr = targetBallMoveDirectionLine;
        lr.positionCount = 2;
        lr.SetPosition(0, targetBallPosition);
        lr.SetPosition(1, targetBallPosition + new Vector3(targetBallDirection.x, 0, targetBallDirection.y) * targetBallLineLength);

        lr = aimDirectionLine;
        lr.positionCount = 2;
        lr.SetPosition(0, cueStickPivot.transform.position);
        lr.SetPosition(1, hitPosition);

        imaginationBall.transform.position = hitPosition;
    }

    public void DetectDangerousCircles()
    {
        dangerousCircles.Clear();
        ballObjects.Clear();

        Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        
        for(int i = 0; i < rigidbodies.Length; i++)
        {
            GameObject ball = rigidbodies[i].gameObject;
            ballObjects.Add(ball);
            Vector2 ballCenter2D = To2D(ball.transform.position);
            float radius = cueBallRadius * 2;

            Circle ballCircle = new Circle(ballCenter2D, radius);
            dangerousCircles.Add(ballCircle);
        }

        dangerousCircles = SortCirclesByDistanceWithCueBall(dangerousCircles);
    }

    public void ChangeLineColor(Color directEmissionColor, Color targetDirectEmissionColor, Color cueDirectEmissionColor, float emissionIntensity)
    {
        Color directEmission = directEmissionColor * Mathf.LinearToGammaSpace(emissionIntensity);
        Color cueEmission = cueDirectEmissionColor * Mathf.LinearToGammaSpace(emissionIntensity);
        Color targetEmission = targetDirectEmissionColor * Mathf.LinearToGammaSpace(emissionIntensity);

        aimDirectionLine.material.SetColor("_EmissionColor", directEmission);
        cueBallMoveDirectionLine.material.SetColor("_EmissionColor", cueEmission);
        targetBallMoveDirectionLine.material.SetColor("_EmissionColor", targetEmission);
        imaginationBall.GetComponent<Renderer>().material.SetColor("_Color", directEmissionColor);

        aimDirectionLine.material.EnableKeyword("_EMISSION");
        cueBallMoveDirectionLine.material.EnableKeyword("_EMISSION");
        targetBallMoveDirectionLine.material.EnableKeyword("_EMISSION");
    }

    private void HandleCircleInteractions(Vector3 cueBallPos, Vector3 aimDirection)
    {
        Vector2 cueBall2D = To2D(cueBallPos);
        Vector2 aimDirection2D = To2D(aimDirection);

        StraightRay2D aimRay = new StraightRay2D(cueBall2D, aimDirection2D);

        bool hitCircle = false;
        foreach(var circle in dangerousCircles)
        {
            Vector2? cutpoint = circle.Cutpoint(aimRay);

            if (cutpoint != null)
            {
                Vector3 hitPoint = To3D((Vector2)cutpoint, cueBallPos.y);
                aimDirectionLine.SetPosition(1, hitPoint);

                imaginationBall.transform.position = hitPoint;
                imaginationBall.SetActive(true);

                GameObject closestBall = null;
                float closestDistanceSqr = Mathf.Infinity;

                foreach (var ball in ballObjects)
                {
                    if (ball != null)
                    {
                        Collider ballCollider = ball.GetComponent<Collider>();

                        if (ballCollider != null)
                        {
                            Vector3 ballCenter = ballCollider.bounds.center;
                            float distanceSqr = (hitPoint - ballCenter).sqrMagnitude;

                            if (distanceSqr < closestDistanceSqr)
                            {
                                closestDistanceSqr = distanceSqr;
                                closestBall = ball;
                            }
                        }
                    }
                }

                if (closestBall != null)
                {
                    Collider closestBallCollider = closestBall.GetComponent<Collider>();

                    if (closestBallCollider != null)
                    {
                        Vector2 cueballPotentialDirection = Vector2.Perpendicular(new Vector2(hitPoint.x, hitPoint.z));
                        Vector2 cueballDirection = (aimDirection2D + cueballPotentialDirection).magnitude > (aimDirection2D - cueballPotentialDirection).magnitude ? cueballPotentialDirection : -cueballPotentialDirection;
                        cueballDirection.Normalize();

                        Vector2 targetBallDirection = -(new Vector2(hitPoint.x, hitPoint.z) - new Vector2(closestBallCollider.bounds.center.x, closestBallCollider.bounds.center.z)).normalized;

                        if (twoPlayerPocketScript) twoPlayerPocketScript.CheckLineColor(closestBall);

                        UpdateAimVisualizeComponent(hitPoint, closestBallCollider.bounds.center, cueballDirection, targetBallDirection);
                    }
                }

                hitCircle = true;
                break;
            }
        }

        if(!hitCircle)
        {
            RaycastHit tableHit;
            if(Physics.Raycast(cueBallPos, aimDirection, out tableHit, 100f))
            {
                Vector3 adjustedHitPoint = tableHit.point - aimDirection * cueBallRadius;

                aimDirectionLine.SetPosition(1, adjustedHitPoint);

                imaginationBall.transform.position = adjustedHitPoint;
                imaginationBall.SetActive(true);

                Vector2 adjustedHitPoint2D = To2D(adjustedHitPoint);
                Vector2 tableNormal2D = To2D(tableHit.normal);

                Vector2 tableEdgeDirection = Vector2.Perpendicular(tableNormal2D);
                LineSegment2D tableEdgeSegment = new LineSegment2D(
                    adjustedHitPoint2D - tableEdgeDirection * 0.5f,
                    adjustedHitPoint2D + tableEdgeDirection * 0.5f
                );

                Vector2 cueBallReflection2D = tableEdgeSegment.ReflectVector(aimDirection2D);
                Vector3 cueBallReflection = To3D(cueBallReflection2D, cueBallPos.y);

                ChangeLineColor(Color.white, Color.white, Color.white, whiteEmission);

                if (tableHit.collider.gameObject.CompareTag("Table"))
                    UpdateAimVisualizeComponent(adjustedHitPoint, Vector3.zero, cueBallReflection2D, Vector2.zero);

                if (tableHit.collider.gameObject.CompareTag("TablePocket"))
                    UpdateAimVisualizeComponent(adjustedHitPoint, Vector3.zero, aimDirection2D * 0.001f, Vector2.zero);
            }
        }
    }

    public static Vector2 To2D(Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }

    public static Vector3 To3D(Vector2 vec, float y)
    {
        return new Vector3(vec.x, y, vec.y);
    }

    public List<Circle> SortCirclesByDistanceWithCueBall(List<Circle> dangerousCircles)
    {
        Vector2 cueBall2D = To2D(cueStickPivot.transform.position);
        return SortCirclesByDistanceWithPoint(dangerousCircles, cueBall2D);
    }

    public List<Circle> SortCirclesByDistanceWithPoint(List<Circle> dangerousCircles, Vector2 rawPosition)
    {
        dangerousCircles.Sort((c1, c2) =>
            Vector2.Distance(c1.center, rawPosition).CompareTo(Vector2.Distance(c2.center, rawPosition))
        );
        return dangerousCircles;
    }

    public struct Circle
    {
        public Vector2 center;
        public float radius;

        public Circle(Vector2 center, float radius)
        {
            this.center = center; ;
            this.radius = radius;
        }

        public bool IsContain(Vector2 point)
        {
            return Vector2.Distance (center, point) <= radius;
        }

        public Vector2? Cutpoint(StraightRay2D ray)
        {
            Vector2 prjCenterOnAimLine = (Vector2)Vector3.Project(center - ray.start, ray.direction) + ray.start;
            if(IsContain(prjCenterOnAimLine))
            {
                float disFromCenterToPrj = Vector2.Distance(prjCenterOnAimLine, center);
                float disFromPrjToHitPoint = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(disFromCenterToPrj, 2));
                Vector2 hitPosition = prjCenterOnAimLine - (ray.direction.normalized * disFromPrjToHitPoint);
                if(ray.IsContain(hitPosition))
                {
                    return hitPosition;
                }
            }
            return null;
        }
    }

    public class StraightRay2D
    {
        public Vector2 start;
        public Vector2 direction;

        public StraightRay2D(Vector2 start, Vector2 direction)
        {
            this.start = start;
            this.direction = direction;
        }

        public bool IsContain(Vector2 point)
        {
            Vector2 toPoint = point - start;
            float angleErrorMargin = 0.1f;
            return Vector2.Angle(toPoint, direction) < angleErrorMargin;
        }
    }

    public class LineSegment2D
    {
        public Vector2 start;
        public Vector2 end;

        public LineSegment2D(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }

        public Vector2 ReflectVector(Vector2 vec)
        {
            Vector2 prj = (Vector2)Vector3.Project(vec, end - start);
            return vec + 2 * (prj - vec);
        }
    }
}
