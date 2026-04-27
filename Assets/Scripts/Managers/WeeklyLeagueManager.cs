using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeeklyLeagueManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI rewardText;

    private DateTime leagueEndTime;
    private int weeklyReward;

    void Start()
    {
        FetchLeagueData();
    }

    void Update()
    {
        UpdateTimer();
    }

    void FetchLeagueData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            titleResult =>
            {
                weeklyReward = int.Parse(titleResult.Data["WeeklyLeagueReward"]);
                rewardText.text = $"Reward: {weeklyReward}";

                FetchPlayerLeagueData();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void FetchPlayerLeagueData()
    {
        PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("LeagueEndTime"))
                {
                    leagueEndTime = DateTime.Parse(result.Data["LeagueEndTime"].Value);
                }
                else
                {
                    StartNewLeague();
                }
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void StartNewLeague()
    {
        PlayFabClientAPI.GetTime(new GetTimeRequest(),
            timeResult =>
            {
                leagueEndTime = timeResult.Time.AddDays(7);

                PlayFabClientAPI.UpdateUserData(
                    new UpdateUserDataRequest
                    {
                        Data = new Dictionary<string, string>
                        {
                            { "LeagueEndTime", leagueEndTime.ToString("o") },
                            { "LeagueClaimed", "false" }
                        },
                        Permission = UserDataPermission.Public
                    },
                    result =>
                    {
                        Debug.Log("Weekly league data initialized");
                    },
                    error =>
                    {
                        Debug.LogError(error.GenerateErrorReport());
                    }
                );
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void UpdateTimer()
    {
        if (leagueEndTime == default) return;

        TimeSpan remaining = leagueEndTime - DateTime.UtcNow;

        if (remaining.TotalSeconds <= 0)
        {
            timerText.text = "Ends In: 0h 0m 0s";
            return;
        }

        int totalHours = (int)remaining.TotalHours;
        int minutes = remaining.Minutes;
        int seconds = remaining.Seconds;

        timerText.text = $"Ends In: {totalHours}h {minutes}m {seconds}s";
    }
}
