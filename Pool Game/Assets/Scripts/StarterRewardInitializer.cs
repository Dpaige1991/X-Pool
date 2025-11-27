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
                        UpdateProgressUI(savedProgress);
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
            UpdateProgressUI(starterProgressPercent);  // <-- ADDED
        },
        error =>
        {
            Debug.LogError("UpdateUserData error: " + error.ErrorMessage);
        });
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
