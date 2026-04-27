using UnityEngine;

public class Pocket : MonoBehaviour
{
    public _PocketId PocketId;

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponentInParent<Ball>();
        if (ball == null) return;

        Debug.Log($"POCKET: Ball {ball.BallNumber} ({ball.GetGroup()}) -> {PocketId}");

        ShotTracker.Instance?.RegisterPocket(ball, PocketId);

        // Cue ball is handled by TurnManager via ShotTracker.OnBallPocketed
        if (ball.IsCue) return;

        ball.gameObject.SetActive(false);
    }
}