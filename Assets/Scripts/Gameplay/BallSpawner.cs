using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform cueSpawn;
    [SerializeField] private Transform rackOrigin; // Apex ball position (front of triangle)
    [SerializeField] private Transform ballsParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject cueBallPrefab;
    [Tooltip("Exactly 15 prefabs (1-15). Order doesn't matter unless you want it to.")]
    [SerializeField] private List<GameObject> objectBallPrefabs = new List<GameObject>(15);

    [Header("Rack Settings")]
    [Tooltip("Ball diameter in world units. If your balls are scaled, set this correctly.")]
    [SerializeField] private float ballDiameter = 0.05715f; // real pool ball ~ 57.15mm, change if your scale differs
    [SerializeField] private float tableUpOffset = 0.001f;   // tiny lift so balls don't spawn intersecting the cloth

    [Tooltip("Rack points along this direction (from apex backwards). Usually toward the foot rail.")]
    [SerializeField] private Vector3 rackBackDirection = Vector3.back;

    [Tooltip("Rack spreads left/right along this direction. Usually table left/right.")]
    [SerializeField] private Vector3 rackSideDirection = Vector3.right;

    [Header("8-ball Rules (Optional)")]
    [SerializeField] private bool enforceEightBallCenter = true;
    [SerializeField] private bool enforceCornerSolidStripe = true;

    private readonly List<GameObject> _spawned = new();

    [ContextMenu("Spawn New Rack")]
    public void SpawnNewRack()
    {
        ClearSpawned();

        if (cueBallPrefab == null || cueSpawn == null)
        {
            Debug.LogError("[BallSpawner] Missing cueBallPrefab or cueSpawn.");
            return;
        }

        if (rackOrigin == null)
        {
            Debug.LogError("[BallSpawner] Missing rackOrigin.");
            return;
        }

        if (objectBallPrefabs == null || objectBallPrefabs.Count != 15)
        {
            Debug.LogError("[BallSpawner] objectBallPrefabs must contain exactly 15 prefabs.");
            return;
        }

        // Spawn cue
        SpawnBall(cueBallPrefab, cueSpawn.position, cueSpawn.rotation, "CueBall");

        // Build rack order (optionally enforce rules)
        var rackOrder = BuildRackOrder(objectBallPrefabs);

        // Spawn object balls in triangle
        var positions = GenerateTriangleRackPositions(
            rackOrigin.position,
            rackOrigin.rotation,
            ballDiameter,
            rackBackDirection.normalized,
            rackSideDirection.normalized
        );

        for (int i = 0; i < 15; i++)
        {
            var prefab = rackOrder[i];
            SpawnBall(prefab, positions[i], rackOrigin.rotation, prefab.name);
        }
    }

    private void SpawnBall(GameObject prefab, Vector3 pos, Quaternion rot, string debugName)
    {
        pos.y += tableUpOffset;

        var go = Instantiate(prefab, pos, rot, ballsParent);
        go.name = debugName;

        // Safety checks
        if (!go.TryGetComponent<Rigidbody>(out _))
            Debug.LogWarning($"[BallSpawner] {go.name} has no Rigidbody.");
        if (!go.TryGetComponent<Collider>(out _))
            Debug.LogWarning($"[BallSpawner] {go.name} has no Collider.");

        _spawned.Add(go);
    }

    public void ClearSpawned()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null) Destroy(_spawned[i]);
        }
        _spawned.Clear();
    }

    // Generates 15 positions for a standard 5-row triangle:
    // Row sizes: 1,2,3,4,5 = 15
    private List<Vector3> GenerateTriangleRackPositions(
        Vector3 apexWorld,
        Quaternion rackRotation,
        float diameter,
        Vector3 backDirLocal,
        Vector3 sideDirLocal
    )
    {
        // Convert the provided directions into world-space based on rackRotation
        Vector3 backDir = rackRotation * backDirLocal; // points from apex to back
        Vector3 sideDir = rackRotation * sideDirLocal; // left/right spread

        // In a tight rack, adjacent centers are separated by diameter.
        // Row spacing along back direction = diameter * sqrt(3)/2
        float rowSpacing = diameter * 0.8660254f; // sqrt(3)/2

        var result = new List<Vector3>(15);

        int index = 0;
        for (int row = 0; row < 5; row++)
        {
            int ballsInRow = row + 1;
            float rowOffsetBack = row * rowSpacing;

            // Center the row horizontally around the triangle centerline
            // Horizontal spacing = diameter
            float rowWidth = (ballsInRow - 1) * diameter;

            for (int col = 0; col < ballsInRow; col++)
            {
                float sideOffset = -rowWidth * 0.5f + col * diameter;

                Vector3 p = apexWorld + (backDir * rowOffsetBack) + (sideDir * sideOffset);
                result.Add(p);
                index++;
            }
        }

        return result;
    }

    // Rack indexing (our generated order) is:
    // Row 0: [0]
    // Row 1: [1,2]
    // Row 2: [3,4,5]
    // Row 3: [6,7,8,9]
    // Row 4: [10,11,12,13,14]
    //
    // Standard 8-ball rules:
    // - 8 ball in center of row 2 => index 4
    // - Back corners are one solid + one stripe => indices 10 and 14 (corners of last row)
    private List<GameObject> BuildRackOrder(List<GameObject> input)
    {
        var pool = new List<GameObject>(input);

        // If you don't care about specific rules, just shuffle
        if (!enforceEightBallCenter && !enforceCornerSolidStripe)
        {
            Shuffle(pool);
            return pool;
        }

        // We need to identify 8-ball, solids, stripes by name or component.
        // QUICK approach: name contains "8" for eight-ball, and "Stripe"/"Solid" keywords.
        // You can replace this with a Ball script having BallNumber/BallType.
        GameObject eight = FindByPredicate(pool, p => p.name.Contains("8"));
        if (eight == null)
        {
            Debug.LogWarning("[BallSpawner] enforceEightBallCenter is on, but couldn't find an '8' ball prefab by name. (Name should contain '8')");
        }

        var solids = new List<GameObject>();
        var stripes = new List<GameObject>();
        var unknown = new List<GameObject>();

        foreach (var p in pool)
        {
            var n = p.name.ToLowerInvariant();
            if (n.Contains("stripe")) stripes.Add(p);
            else if (n.Contains("solid")) solids.Add(p);
            else unknown.Add(p);
        }

        // Fallback if you didn't label prefabs with Solid/Stripe
        if (enforceCornerSolidStripe && (solids.Count == 0 || stripes.Count == 0))
        {
            Debug.LogWarning("[BallSpawner] Could not detect solids/stripes by prefab name. Either rename prefabs to include 'Solid'/'Stripe' or turn off enforceCornerSolidStripe.");
        }

        // Start with shuffle for randomness
        Shuffle(pool);

        // Put 8 ball in center
        if (enforceEightBallCenter && eight != null)
        {
            pool.Remove(eight);
            // center index in our rack is 4
            pool.Insert(4, eight);
        }

        // Corner rule: last row corners are indices 10 and 14
        if (enforceCornerSolidStripe && solids.Count > 0 && stripes.Count > 0)
        {
            // Remove one solid + one stripe from pool
            GameObject solidCorner = solids[Random.Range(0, solids.Count)];
            GameObject stripeCorner = stripes[Random.Range(0, stripes.Count)];

            pool.Remove(solidCorner);
            pool.Remove(stripeCorner);

            // If index shifts occurred due to earlier insert/remove, ensure we place safely:
            // We'll rebuild a final list by setting specific indices.
            var final = new List<GameObject>(pool);
            // Ensure size is 15
            if (final.Count > 15) final.RemoveRange(15, final.Count - 15);
            while (final.Count < 15) final.Add(input[0]); // shouldn't happen

            final[10] = solidCorner;
            final[14] = stripeCorner;

            // If 8-ball must be center, keep it there (just in case corner placement overwrote something)
            if (enforceEightBallCenter && eight != null)
                final[4] = eight;

            return final;
        }

        // Default if we couldn't enforce corner rule
        if (pool.Count != 15)
        {
            // Ensure correct size
            while (pool.Count > 15) pool.RemoveAt(pool.Count - 1);
            while (pool.Count < 15) pool.Add(input[0]);
        }

        return pool;
    }

    private static GameObject FindByPredicate(List<GameObject> list, System.Predicate<GameObject> predicate)
    {
        for (int i = 0; i < list.Count; i++)
            if (predicate(list[i])) return list[i];
        return null;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}