using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Player 1 UI")]
    public TMP_Text p1BallsPocketedText;
    public TMP_Text p1AccuracyText;
    public TMP_Text p1BestComboText;
    public TMP_Text p1FoulsText;

    [Header("Player 2 UI")]
    public TMP_Text p2BallsPocketedText;
    public TMP_Text p2AccuracyText;
    public TMP_Text p2BestComboText;
    public TMP_Text p2FoulsText;

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        var p1 = PlayerStats.Instance.GetStats(_PlayerId.Player1);
        var p2 = PlayerStats.Instance.GetStats(_PlayerId.Player2);

        // PLAYER 1
        p1BallsPocketedText.text = $"Balls: {p1.BallsPocketed}";
        p1AccuracyText.text = $"Accuracy: {p1.Accuracy:F1}%";
        p1BestComboText.text = $"Best Combo: {p1.BestCombo}";
        p1FoulsText.text = $"Fouls: {p1.Fouls}";

        // PLAYER 2
        p2BallsPocketedText.text = $"Balls: {p2.BallsPocketed}";
        p2AccuracyText.text = $"Accuracy: {p2.Accuracy:F1}%";
        p2BestComboText.text = $"Best Combo: {p2.BestCombo}";
        p2FoulsText.text = $"Fouls: {p2.Fouls}";
    }
}