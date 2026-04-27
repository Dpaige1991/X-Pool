using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayerChallengeUI : MonoBehaviour
{
    [Header("Challenge UI")]
    public GameObject sendChallengeUI;
    public Button sendButton;

    [Header("Arena Data")]
    public LevelData[] levels;

    [Header("Scroll Controls")]
    public RectTransform arenaContent;
    public Button leftButton;
    public Button rightButton;
    public Button closeButton;

    private Vector2 leftPosition = new Vector2(-600, 0);
    private Vector2 rightPosition = new Vector2(75, 0);

    private string targetPlayFabId;
    private LevelData selectedLevel;
    private string challengerName;

    void Start()
    {
        leftButton.onClick.AddListener(SelectLeftArena);
        rightButton.onClick.AddListener(SelectRightArena);
        closeButton.onClick.AddListener(CloseMenu);

        arenaContent.anchoredPosition = rightPosition;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(false);

        sendButton.onClick.AddListener(SendChallenge);

        // Get local player's display name
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), result =>
        {
            challengerName = result.AccountInfo.TitleInfo.DisplayName ?? "Player";
        },
        error =>
        {
            challengerName = "Player";
        });
    }

    public void Setup(string playFabId)
    {
        targetPlayFabId = playFabId;
    }

    void SelectLeftArena()
    {
        arenaContent.anchoredPosition = leftPosition;
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(true);

        SetSelectedLevel(0);
    }

    void SelectRightArena()
    {
        arenaContent.anchoredPosition = rightPosition;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(false);

        SetSelectedLevel(1);
    }

    void SetSelectedLevel(int index)
    {
        if (index < 0 || index >= levels.Length)
            return;

        selectedLevel = levels[index];
    }

    void SendChallenge()
    {
        if (selectedLevel == null)
        {
            Debug.LogWarning("No level selected");
            return;
        }

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "SendChallenge",
            FunctionParameter = new
            {
                TargetId = targetPlayFabId,
                Arena = selectedLevel.levelId,
                Fee = selectedLevel.entryFee,
                Challenger = challengerName
            },
            GeneratePlayStreamEvent = true
        },
        result =>
        {
            Debug.Log("Challenge sent");
        },
        error =>
        {
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    void CloseMenu()
    {
        this.gameObject.SetActive(false);
    }
}
