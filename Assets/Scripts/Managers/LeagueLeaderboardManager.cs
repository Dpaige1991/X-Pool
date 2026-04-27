using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeagueLeaderboardManager : MonoBehaviour
{
    public enum LeaderboardCategory
    {
        Friends,
        Region,
        World
    }

    [Header("UI")]
    public Transform contentParent;
    public GameObject leaderboardEntryPrefab;

    [Header("Settings")]
    public string statisticName = "WeeklyLeagueScore";
    public int maxResults = 50;

    private CountryCode? playerRegion;

    void Start()
    {
        FetchPlayerRegion();
    }

    // ---------- PUBLIC BUTTON HOOKS ----------
    public void ShowFriends()
    {
        LoadLeaderboard(LeaderboardCategory.Friends);
    }

    public void ShowRegion()
    {
        LoadLeaderboard(LeaderboardCategory.Region);
    }

    public void ShowWorld()
    {
        LoadLeaderboard(LeaderboardCategory.World);
    }

    // ---------- CORE ----------
    void LoadLeaderboard(LeaderboardCategory category)
    {
        ClearEntries();

        switch (category)
        {
            case LeaderboardCategory.Friends:
                LoadFriendsLeaderboard();
                break;

            case LeaderboardCategory.Region:
                LoadRegionLeaderboard();
                break;

            case LeaderboardCategory.World:
                LoadWorldLeaderboard();
                break;
        }
    }

    // ---------- WORLD ----------
    void LoadWorldLeaderboard()
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = maxResults
        },
        OnLeaderboardSuccess,
        OnError);
    }

    // ---------- FRIENDS ----------
    void LoadFriendsLeaderboard()
    {
        PlayFabClientAPI.GetFriendLeaderboard(new GetFriendLeaderboardRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = maxResults
        },
        OnLeaderboardSuccess,
        OnError);
    }

    // ---------- REGION ----------
    void LoadRegionLeaderboard()
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = maxResults,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowLocations = true
            }
        },
        result =>
        {
            foreach (var entry in result.Leaderboard)
            {
                if (entry.Profile?.Locations == null) continue;

                CountryCode? country = entry.Profile.Locations[0].CountryCode;

                if (country.HasValue && playerRegion.HasValue && country.Value == playerRegion.Value)
                {
                    CreateEntry(entry);
                }
            }
        },
        OnError);
    }

    // ---------- HELPERS ----------
    void OnLeaderboardSuccess(GetLeaderboardResult result)
    {
        foreach (var entry in result.Leaderboard)
        {
            CreateEntry(entry);
        }
    }

    void SetMedal(Transform entryTransform, int rank)
    {
        Transform gold = entryTransform.Find("GoldMedal");
        Transform silver = entryTransform.Find("SilverMedal");
        Transform bronze = entryTransform.Find("BronzeMedal");

        if (gold) gold.gameObject.SetActive(rank == 1);
        if (silver) silver.gameObject.SetActive(rank == 2);
        if (bronze) bronze.gameObject.SetActive(rank == 3);
    }


    void CreateEntry(PlayerLeaderboardEntry entry)
    {
        GameObject obj = Instantiate(leaderboardEntryPrefab, contentParent);

        int rank = entry.Position + 1;

        obj.transform.Find("RankText")
            .GetComponent<TextMeshProUGUI>().text = rank.ToString();

        obj.transform.Find("NameText")
            .GetComponent<TextMeshProUGUI>().text = entry.DisplayName ?? "Player";

        obj.transform.Find("ScoreText")
            .GetComponent<TextMeshProUGUI>().text = entry.StatValue.ToString();

        SetMedal(obj.transform, rank);
    }

    void ClearEntries()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    void FetchPlayerRegion()
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
        {
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowLocations = true
            }
        },
        result =>
        {
            if (result.PlayerProfile.Locations != null)
            {
                playerRegion = result.PlayerProfile.Locations[0].CountryCode;
            }
        },
        OnError);
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}
