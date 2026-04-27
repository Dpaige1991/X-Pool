using UnityEngine;

[System.Serializable]
public class DailyMission
{
    public string missionName;
    public int goalAmount;          // How much progress is needed
    public int currentAmount;       // Current progress
    public int gemReward;           // Reward in gems
    public bool isCompleted => currentAmount >= goalAmount;
}
