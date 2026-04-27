// Assets/_Project/Scripts/Core/ShotState.cs
using System;
using UnityEngine;

public class ShotState : MonoBehaviour
{
    public MatchState State { get; private set; } = MatchState.OpenTable;
    public event Action<MatchState> OnStateChanged;

    public bool CanAim => State != MatchState.BallsMoving && State != MatchState.GameOver;

    public void SetState(MatchState s)
    {
        if (State == s) return;
        State = s;
        OnStateChanged?.Invoke(State);
    }
}