using UnityEngine;

public enum BallGroup
{
    None,
    Solids,
    Stripes
}

public class Ball : MonoBehaviour
{
    [Header("Ball Info")]
    [Range(0, 15)] public int BallNumber; // 0 = cue, 8 = eight, 1-7 solids, 9-15 stripes

    public bool IsCue => BallNumber == 0;
    public bool IsEight => BallNumber == 8;

    public BallGroup GetGroup()
    {
        if (IsCue || IsEight) return BallGroup.None;
        if (BallNumber >= 1 && BallNumber <= 7) return BallGroup.Solids;
        if (BallNumber >= 9 && BallNumber <= 15) return BallGroup.Stripes;

        return BallGroup.None;
    }
}