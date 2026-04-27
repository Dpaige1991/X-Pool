using UnityEngine;
using System.Collections.Generic;

public enum _PlayerId { Player1, Player2 }

[System.Serializable]
public class PlayerStatData
{
    public int BallsPocketed;
    public int ShotsTaken;

    // Successful SHOTS (not balls)
    public int SuccessfulShots;

    public int BestCombo;
    public int CurrentCombo;
    public int Fouls;

    public float Accuracy
    {
        get
        {
            if (ShotsTaken == 0) return 0f;
            return (float)SuccessfulShots / ShotsTaken * 100f;
        }
    }

    public void ResetTurnCombo() => CurrentCombo = 0;
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    private readonly Dictionary<_PlayerId, PlayerStatData> stats =
        new Dictionary<_PlayerId, PlayerStatData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        stats[_PlayerId.Player1] = new PlayerStatData();
        stats[_PlayerId.Player2] = new PlayerStatData();
    }

    public PlayerStatData GetStats(_PlayerId player) => stats[player];

    // Always call when the player actually shoots
    public void RegisterShot(_PlayerId player)
    {
        stats[player].ShotsTaken++;
        // NOTE: don't reset combo here; combo resets on miss/foul/turn end.
    }

    // Call ONCE at end of shot if it was a legal made shot
    public void RegisterSuccessfulShot(_PlayerId player)
    {
        stats[player].SuccessfulShots++;
    }

    // Call at end of shot if no pocket and no foul
    public void RegisterMiss(_PlayerId player)
    {
        stats[player].ResetTurnCombo();
    }

    // Call per ball pocketed (non-cue). Drives balls count + combo.
    public void RegisterBallPocketed(_PlayerId player)
    {
        var s = stats[player];
        s.BallsPocketed++;
        s.CurrentCombo++;

        if (s.CurrentCombo > s.BestCombo)
            s.BestCombo = s.CurrentCombo;
    }

    // Call on foul
    public void RegisterFoul(_PlayerId player)
    {
        stats[player].Fouls++;
        stats[player].ResetTurnCombo();
    }

    // Call when turn switches
    public void OnTurnEnded(_PlayerId player)
    {
        stats[player].ResetTurnCombo();
    }
}