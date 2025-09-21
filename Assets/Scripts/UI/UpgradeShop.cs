using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeShop : MonoBehaviour
{
    public static UpgradeShop Instance;
    
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform contentBox;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Button closeButton;
    
    [Header("Shop Settings")]
    [SerializeField] private int upgradesPerShop = 3;
    
    private List<UpgradeButton> currentUpgradeButtons = new List<UpgradeButton>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        
        // Start with shop closed
        shopPanel.SetActive(false);
    }
    
    public void OpenShop()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogError("UpgradeManager not found!");
            return;
        }
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Get random upgrades
        List<UpgradeData> randomUpgrades = UpgradeManager.Instance.GetRandomUpgrades(upgradesPerShop);
        
        // Clear existing buttons
        ClearUpgradeButtons();
        
        // Create upgrade buttons
        CreateUpgradeButtons(randomUpgrades);
        
        // Show shop
        shopPanel.SetActive(true);
        
        Debug.Log("Upgrade shop opened with " + randomUpgrades.Count + " options");
    }
    
    private void ClearUpgradeButtons()
    {
        foreach (var button in currentUpgradeButtons)
        {
            if (button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }
        currentUpgradeButtons.Clear();
    }
    
    private void CreateUpgradeButtons(List<UpgradeData> upgrades)
    {
        foreach (var upgrade in upgrades)
        {
            GameObject buttonObj = Instantiate(upgradeButtonPrefab, contentBox);
            UpgradeButton upgradeButton = buttonObj.GetComponent<UpgradeButton>();
            
            if (upgradeButton != null)
            {
                upgradeButton.Setup(upgrade, this);
                currentUpgradeButtons.Add(upgradeButton);
            }
            else
            {
                Debug.LogError("UpgradeButton component not found on upgrade button prefab!");
            }
        }
    }
    
    public void SelectUpgrade(UpgradeData selectedUpgrade)
    {
        // Apply the upgrade
        UpgradeManager.Instance.ApplyUpgrade(selectedUpgrade);
        
        // Close shop and resume game
        CloseShop();
        
        // Notify GameManager that upgrade was selected
        GameManager.Instance.OnUpgradeSelected();
        
        Debug.Log($"Selected upgrade: {selectedUpgrade.upgradeName}");
    }
    
    public void CloseShop()
    {
        // Hide shop
        shopPanel.SetActive(false);
        
        // Clear buttons
        ClearUpgradeButtons();
        
        // Resume game
        Time.timeScale = 1f;
        
        Debug.Log("Upgrade shop closed");
    }
}