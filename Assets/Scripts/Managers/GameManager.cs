using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text brainText;
    [SerializeField] private Slider brainProgressBar;

    [Header("Game Settings")]
    [SerializeField] private float gameTime = 300f;
    
    [Header("Upgrade System")]
    [SerializeField] private int baseBrainsRequired = 5;
    [SerializeField] private int brainsRequiredIncrease = 3;
    [SerializeField] private int maxBrainsRequired = 25;

    private float timeLeft;
    private int brainsEaten = 0;
    private int totalBrainsEaten = 0;
    private int currentBrainsRequired;
    private int upgradeLevel = 1;
    private bool gameOver = false;

    public static Action OnGameLost;
    public static Action OnUpgradeAvailable;

    private void Awake()
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
    }

    private void Start()
    {
        timeLeft = gameTime;
        currentBrainsRequired = baseBrainsRequired;
        UpdateUI();
    }

    private void Update()
    {
        if (gameOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            LoseGame();
        }
        UpdateUI();
    }

    public void AddBrain()
    {
        brainsEaten++;
        totalBrainsEaten ++;
        
        // Check if we've reached the required brains for upgrade
        if (brainsEaten >= currentBrainsRequired)
        {
            TriggerUpgradeShop();
        }
        
        UpdateUI();
    }

    private void TriggerUpgradeShop()
    {
        Debug.Log($"Upgrade available! Level {upgradeLevel}");
        
        // Open upgrade shop
        if (UpgradeShop.Instance != null)
        {
            UpgradeShop.Instance.OpenShop();
        }
        else
        {
            Debug.LogError("UpgradeShop not found!");
        }
        
        OnUpgradeAvailable?.Invoke();
    }

    public void OnUpgradeSelected()
    {
        // Reset progress bar
        brainsEaten = 0;
        upgradeLevel++;
        
        // Increase brains required for next upgrade
        currentBrainsRequired = Mathf.Min(
            baseBrainsRequired + (brainsRequiredIncrease * (upgradeLevel - 1)), 
            maxBrainsRequired
        );
        
        Debug.Log($"Upgrade level {upgradeLevel}, next upgrade needs {currentBrainsRequired} brains");
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Timer
        timerText.text = Mathf.Ceil(timeLeft).ToString();
        
        // Brain counter
        brainText.text = "Brains Eaten: " + totalBrainsEaten;
        
        // Progress bar
        if (brainProgressBar != null)
        {
            float progress = (float)brainsEaten / currentBrainsRequired;
            brainProgressBar.value = progress;
        }
    }

    // Methods that upgrades can call
    public void AddTime(float additionalTime)
    {
        timeLeft += additionalTime;
        Debug.Log($"Added {additionalTime} seconds to timer");
    }

    public void ReduceBrainRequirement(int reduction)
    {
        currentBrainsRequired = Mathf.Max(1, currentBrainsRequired - reduction);
        Debug.Log($"Reduced brain requirement by {reduction}. New requirement: {currentBrainsRequired}");
        UpdateUI();
    }

    public void WinGame()
    {
        gameOver = true;
        Debug.Log("You Win!");
    }

    public void LoseGame()
    {
        gameOver = true;
        Debug.Log("You Lose!");
        OnGameLost?.Invoke();
    }

    // Getters for other systems
    public int GetCurrentBrainsRequired() => currentBrainsRequired;
    public int GetBrainsEaten() => brainsEaten;
    public int GetUpgradeLevel() => upgradeLevel;
    public float GetTimeLeft() => timeLeft;
}