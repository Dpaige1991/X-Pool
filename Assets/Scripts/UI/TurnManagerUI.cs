using System.Collections;
using TMPro;
using UnityEngine;

public class TurnManagerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;

    [Header("UI")]
    [SerializeField] private GameObject inGameTextObject;   // Parent object (InGameText)
    [SerializeField] private TMP_Text groupAssignmentText;  // Child TMP

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;

    private Coroutine hideRoutine;

    private void Reset()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();
    }

    private void Awake()
    {
        // Ensure hidden at start
        if (inGameTextObject != null)
            inGameTextObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (turnManager == null) return;

        turnManager.OnGroupsAssignedUI += HandleGroupsAssigned;
    }

    private void OnDisable()
    {
        if (turnManager == null) return;

        turnManager.OnGroupsAssignedUI -= HandleGroupsAssigned;
    }

    private void HandleGroupsAssigned(BallGroup p1, BallGroup p2)
    {
        if (turnManager.TableOpen) return;

        string p1Line = $"{turnManager.Player1Name}, you are {Nice(turnManager.P1Group)}";
        string p2Line = $"{turnManager.Player2Name}, you are {Nice(turnManager.P2Group)}";

        groupAssignmentText.text = $"{p1Line}\n{p2Line}";

        ShowTemporarily();
    }

    private void ShowTemporarily()
    {
        if (inGameTextObject == null) return;

        inGameTextObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (inGameTextObject != null)
            inGameTextObject.SetActive(false);

        hideRoutine = null;
    }

    private string Nice(BallGroup g)
    {
        return g switch
        {
            BallGroup.Solids => "SOLIDS",
            BallGroup.Stripes => "STRIPES",
            _ => "NONE"
        };
    }
}