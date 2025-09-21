using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button button;
    
    private UpgradeData upgradeData;
    private UpgradeShop shop;
    
    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        button.onClick.AddListener(OnButtonClicked);
    }
    
    public void Setup(UpgradeData upgrade, UpgradeShop upgradeShop)
    {
        upgradeData = upgrade;
        shop = upgradeShop;
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (upgradeData == null) return;
        
        // Set name
        if (nameText != null)
        {
            nameText.text = upgradeData.upgradeName;
        }
        
        // Set description with current values
        if (descriptionText != null)
        {
            string description = upgradeData.description;
            
            // Replace placeholders with actual values
            description = description.Replace("{value}", upgradeData.value.ToString());
            description = description.Replace("{intValue}", upgradeData.intValue.ToString());
            
            descriptionText.text = description;
        }
    }

    
    private void OnButtonClicked()
    {
        if (shop != null && upgradeData != null)
        {
            shop.SelectUpgrade(upgradeData);
        }
    }
}