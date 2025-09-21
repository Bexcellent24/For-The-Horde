using System.Collections.Generic;
using UnityEngine;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;
    
    [Header("Upgrade Pool")]
    [SerializeField] private List<UpgradeData> allUpgrades = new List<UpgradeData>();
    
    
    public static Action<UpgradeData> OnUpgradeApplied;
    public static Action OnUpgradeSystemInitialized;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            OnUpgradeSystemInitialized?.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public List<UpgradeData> GetRandomUpgrades(int count = 3)
    {
        Debug.Log($"Total upgrades available: {allUpgrades.Count}");
    
        // Copy all upgrades to available list
        List<UpgradeData> availableUpgrades = new List<UpgradeData>(allUpgrades);
    
        List<UpgradeData> selectedUpgrades = new List<UpgradeData>();
    
        for (int i = 0; i < count && availableUpgrades.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableUpgrades.Count);
            UpgradeData selected = availableUpgrades[randomIndex];
        
            Debug.Log($"Selected upgrade: {selected.upgradeName}");
        
            // Add to selection and remove all instances to avoid duplicates
            selectedUpgrades.Add(selected);
            availableUpgrades.RemoveAll(u => u == selected);
        }
    
        Debug.Log($"Returning {selectedUpgrades.Count} upgrades");
        return selectedUpgrades;
    }
    
    
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        // Apply the upgrade effect
        ApplyUpgradeEffect(upgrade);
        
        OnUpgradeApplied?.Invoke(upgrade);
        
        Debug.Log($"Applied upgrade: {upgrade.upgradeName}");
    }
    
    private void ApplyUpgradeEffect(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.AddTime:
                GameManager.Instance.AddTime(upgrade.value);
                break;
                
            case UpgradeType.FasterHorde:
                if (HordeManager.Instance != null)
                    HordeManager.Instance.IncreaseHordeSpeed(upgrade.value);
                break;
                
            case UpgradeType.SpawnExtraZombies:
                if (HordeManager.Instance != null)
                    HordeManager.Instance.SpawnZombies(upgrade.intValue);
                break;
                
            case UpgradeType.ReduceBrainRequirement:
                GameManager.Instance.ReduceBrainRequirement(upgrade.intValue);
                break;
                
            case UpgradeType.EnemiesHaveWorsePerception:
                ApplyLowerEnemyPerception(upgrade.value);
                break;
        }
    }
    
    private void ApplyLowerEnemyPerception(float perceptionDecrease)
    {
        Debug.Log("Upgrade: Lower Enemy Perception");
        
       
        // Find all zombies only
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Civilian");
    
        foreach (GameObject enemy in enemies)
        {
            EnemyActor enemyActor = enemy.GetComponent<EnemyActor>();
            if (enemyActor != null && enemyActor.data != null)
            {
                enemyActor.EditPerception(perceptionDecrease);
            }
        }
    }
}
