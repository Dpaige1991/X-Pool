using System.Collections.Generic;
using UnityEngine;

public class DailyMissionsManager : MonoBehaviour
{
    public List<DailyMission> dailyMissions;
    public GameObject missionPrefab;   // Prefab for mission UI
    public Transform missionsParent;   // Parent object for the list
    public int epicReward = 100;       // Reward for completing all missions

    private List<MissionUI> missionUIs = new List<MissionUI>();

    private void Start()
    {
        PopulateMissions();
    }

    private void PopulateMissions()
    {
        foreach (var mission in dailyMissions)
        {
            GameObject obj = Instantiate(missionPrefab, missionsParent);
            MissionUI ui = obj.GetComponent<MissionUI>();
            ui.Setup(mission, this);
            missionUIs.Add(ui);
        }
    }

    public void ClaimMission(DailyMission mission)
    {
        if (!mission.isCompleted) return;

        Debug.Log($"Claimed {mission.gemReward} gems for {mission.missionName}!");
        mission.currentAmount = 0; // Reset or mark as claimed
        UpdateEpicReward();
    }

    private void UpdateEpicReward()
    {
        bool allCompleted = true;
        foreach (var m in dailyMissions)
        {
            if (!m.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            Debug.Log($"All missions complete! Claim epic reward: {epicReward} gems!");
            // You can trigger UI popup for epic reward here
        }
    }

    // Call this whenever a mission progresses
    public void AddProgress(DailyMission mission, int amount)
    {
        mission.currentAmount += amount;
        if (mission.currentAmount > mission.goalAmount)
            mission.currentAmount = mission.goalAmount;

        // Update UI
        foreach (var ui in missionUIs)
        {
            if (ui.Mission == mission)
            {
                ui.Refresh();
                break;
            }
        }

        UpdateEpicReward();
    }
}

