using System;

[Serializable]
public class PlayerData
{
    public string playerId;
    public string signInMethod;
    public int level;
    public int coins;
    public int gems;
    public int currentXP;      // Current XP
    public int xpToNextLevel;  // XP required for next level
    public string securityPin;
    public string playerName;
    public string selectedAvatarId;
    public string avatarSprite;

    public PlayerData(string method)
    {
        signInMethod = method;
        level = 1;
        coins = 1000;
        gems = 0;
        currentXP = 0;
        xpToNextLevel = 100; // default XP needed for level 2
        playerName = "New Player";
        selectedAvatarId = "";
        securityPin = "";
        avatarSprite = "";
    }
}