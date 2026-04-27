using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;

    [Header("Illegal 8-ball Message")]
    [SerializeField] private GameObject illegalEightMessageGO; // the message GameObject
    [SerializeField] private TMP_Text illegalEightMessageText;  // optional (can be null)
    [SerializeField] private float illegalMessageSeconds = 2.0f;

    [Header("Game Over Text")]
    [SerializeField] private GameObject gameOverTextGO; // GO that contains "GAME OVER" / final text
    [SerializeField] private TMP_Text gameOverText;      // optional (can be null)

    [Header("Match Summary Canvas")]
    [SerializeField] private GameObject matchSummaryCanvasGO;

    [Header("Options")]
    [SerializeField] private bool hideIllegalMessageAfter = true;

    private Coroutine _routine;

    private void Awake()
    {
        // start hidden
        if (illegalEightMessageGO) illegalEightMessageGO.SetActive(false);
        if (gameOverTextGO) gameOverTextGO.SetActive(false);
        if (matchSummaryCanvasGO) matchSummaryCanvasGO.SetActive(false);
    }

    private void OnEnable()
    {
        if (!turnManager) turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null) turnManager.OnGameOverUI += HandleGameOver;
    }

    private void OnDisable()
    {
        if (turnManager != null) turnManager.OnGameOverUI -= HandleGameOver;
    }

    private void HandleGameOver(GameOverInfo info)
    {
        if (!info.GameOver) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(GameOverSequence(info));
    }

    private IEnumerator GameOverSequence(GameOverInfo info)
    {
        // 1) If illegal 8-ball, show illegal message GO briefly
        if (info.Reason == GameOverReason.EightBallIllegal && illegalEightMessageGO != null)
        {
            if (illegalEightMessageText != null)
            {
                // adjust message text here if you want
                illegalEightMessageText.text = "8-BALL POCKETED ILLEGALLY!";
            }

            illegalEightMessageGO.SetActive(true);

            if (illegalMessageSeconds > 0f)
                yield return new WaitForSeconds(illegalMessageSeconds);

            if (hideIllegalMessageAfter)
                illegalEightMessageGO.SetActive(false);
        }

        // 2) Show GAME OVER text GO
        if (gameOverTextGO != null)
        {
            if (gameOverText != null)
            {
                // Use info.Detail (winner/loser line) or your own formatting
                gameOverText.text = info.Detail;
            }

            gameOverTextGO.SetActive(true);
        }

        // Small pause (optional)
        yield return null;

        // 3) Show Match Summary Canvas
        if (matchSummaryCanvasGO != null)
        {
            matchSummaryCanvasGO.SetActive(true);
        }
    }
}