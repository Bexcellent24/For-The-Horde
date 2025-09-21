using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrades/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Info")]
    public string upgradeName;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Upgrade Type")]
    public UpgradeType upgradeType;
    
    [Header("Values")]
    public float value; 
    public int intValue;
}

public enum UpgradeType
{
    AddTime,           // Adds time to the timer
    FasterHorde,       // Increases horde movement speed
    SpawnExtraZombies, // Spawns additional zombies
    ReduceBrainRequirement, // Reduces brains needed for next upgrade
    EnemiesHaveWorsePerception,  // Improves zombie perception range
}