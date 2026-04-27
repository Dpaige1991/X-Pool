using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance;

    [Header("XP Totals")]
    public int Player1XP;
    public int Player2XP;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void AddXPToPlayer1(int amount)
    {
        if (amount <= 0) return;
        Player1XP += amount;
        Debug.Log($"[ExperienceManager] Player1 XP = {Player1XP} (+{amount})");
    }

    public void AddXPToPlayer2(int amount)
    {
        if (amount <= 0) return;
        Player2XP += amount;
        Debug.Log($"[ExperienceManager] Player2 XP = {Player2XP} (+{amount})");
    }
}