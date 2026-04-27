// Assets/_Project/Scripts/Core/Types.cs
using System;

public enum BallType { Cue, Solid, Stripe, Eight }
public enum PlayerGroup { None, Solids, Stripes }
public enum MatchState
{
    OpenTable,
    AssignedTable,
    CallingEightPocket,
    BallsMoving,
    BallInHand,
    GameOver
}
public enum PocketId { P1, P2, P3, P4, P5, P6 }

[Serializable]
public class PlayerInfo
{
    public string Name;
    public UnityEngine.Sprite Avatar;
    public int Level;
}