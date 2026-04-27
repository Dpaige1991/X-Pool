using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;            // <-- For Slider
using TMPro;                    // <-- For TMP_Text
using PlayFab;
using PlayFab.ClientModels;

public class StarterRewardInitializer : MonoBehaviour
{
    [Header("Starter Values")]
    public int starterCoins = 500; // Set your starting amount here
    public int starterDiamonds = 0;
    public int starterStars = 0;
    public int starterProgressPercent = 0;

    public TMP_Text level;
    public TMP_Text diamonds;
    public TMP_Text coins;
    public TMP_Text playerNameTxt;

    public TMP_Text avatarName;
    public string selectedAvatarName;
    public GameObject avatarButtonContainer;

    public Image mainGameAvatar;
    public Image selectedAvatarPreview;

    [Header("UI References")]
    public Slider progressSlider;     // <-- ADDED

    private const string StarterFlagKey = "StarterGiven";

    public void CheckIfStarterGiven()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey(StarterFlagKey))
                {
                    Debug.Log("Starter rewards already given. Skipping.");

                    // Load saved progress bar value
                    if (result.Data.ContainsKey("ProgressBarPercent"))
                    {
                        int savedProgress = int.Parse(result.Data["ProgressBarPercent"].Value);
                        //UpdateProgressUI(savedProgress);
                        GiveStarterRewards();
                    }
                    return;
                }
                else
                {
                    Debug.Log("No starter rewards found. Giving starter items...");
                    GiveStarterRewards();
                }
            },
            error =>
            {
                Debug.LogError("GetUserData error: " + error.ErrorMessage);
            }
        );
    }

    void GiveStarterRewards()
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            { "Coins", starterCoins.ToString() },
            { "Diamonds", starterDiamonds.ToString() },
            { "Stars", starterStars.ToString() },
            { "ProgressBarPercent", starterProgressPercent.ToString() },
            { StarterFlagKey, "True" }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = data
        },
        result =>
        {
            Debug.Log("Starter rewards saved successfully!");
            level.text = starterStars.ToString();
            diamonds.text = starterDiamonds.ToString();
            coins.text = starterCoins.ToString();
        },
        error =>
        {
            Debug.LogError("UpdateUserData error: " + error.ErrorMessage);
        });
    }

    public void LoadPlayerProgress()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey(StarterFlagKey))
                {
                    Debug.Log("Starter rewards already given. Skipping.");

                    // Load saved progress bar value
                    if (result.Data.ContainsKey("ProgressBarPercent"))
                    {
                        int savedProgress = int.Parse(result.Data["ProgressBarPercent"].Value);
                        UpdateProgressUI(savedProgress);
                    }

                    if (result.Data.ContainsKey("Coins"))
                    {
                        string coinsValue = result.Data["Coins"].Value;
                        coins.text = coinsValue;
                        Debug.Log("Loaded Player Coins: " + coinsValue);
                    }

                    if (result.Data.ContainsKey("Diamonds"))
                    {
                        string diamondsValue = result.Data["Diamonds"].Value;
                        diamonds.text = diamondsValue;
                        Debug.Log("Loaded Player Diamonds: " + diamondsValue);
                    }

                    if (result.Data.ContainsKey("Stars"))
                    {
                        string levelStar = result.Data["Stars"].Value;
                        level.text = levelStar;
                        Debug.Log("Loaded Player Level: " + levelStar);
                    }

                    if (result.Data.ContainsKey("PlayerName"))
                    {
                        string playerName = result.Data["PlayerName"].Value;
                        playerNameTxt.text = playerName;
                        Debug.Log("Loaded Player Name: " + playerName);
                    }
                    return;
                }
                else
                {
                    Debug.Log("No starter rewards found. Giving starter items...");
                }
            },
            error =>
            {
                Debug.LogError("GetUserData error: " + error.ErrorMessage);
            }
        );
    }

    // -----------------------
    // UI UPDATE FUNCTION
    // -----------------------
    void UpdateProgressUI(int percent)
    {
        if (progressSlider != null)
            progressSlider.value = percent / 100f;

        level.text = starterStars.ToString();
        diamonds.text = starterDiamonds.ToString();
        coins.text = starterCoins.ToString();
    }
}
