using UnityEngine;
using UnityEngine.UI;
using TMPro; // Optional, for TextMeshPro

public class MissionUI : MonoBehaviour
{
    public TextMeshProUGUI missionNameText;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;
    public Button claimButton;
    public TextMeshProUGUI rewardText;

    public DailyMission Mission { get; private set; }
    private DailyMissionsManager manager;

    public void Setup(DailyMission mission, DailyMissionsManager manager)
    {
        this.Mission = mission;
        this.manager = manager;

        missionNameText.text = mission.missionName;
        rewardText.text = $"Reward: {mission.gemReward} Gems";

        claimButton.onClick.AddListener(() => manager.ClaimMission(mission));

        Refresh();
    }

    public void Refresh()
    {
        progressSlider.maxValue = Mission.goalAmount;
        progressSlider.value = Mission.currentAmount;
        progressText.text = $"{Mission.currentAmount}/{Mission.goalAmount}";
        claimButton.interactable = Mission.isCompleted;
    }
}
