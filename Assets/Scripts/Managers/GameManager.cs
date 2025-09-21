using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text brainText;
    [SerializeField] private Slider brainProgressBar;

    [Header("Game Settings")]
    [SerializeField] private float gameTime = 300f;
    [SerializeField] private GameObject nukeEffectPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;
    
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
    private bool nukeTriggered = false;

    public static Action OnGameLost;
    public static Action OnGameWon;
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
        
        AudioManager.Instance?.PlayMusic("Background");
    }

    private void Update()
    {
        if (gameOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            StartCoroutine(NukesCoroutine());
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
        OnGameWon?.Invoke();
    }

    public void LoseGame()
    {
        gameOver = true;
        Debug.Log("You Lose!");
        OnGameLost?.Invoke();
    }

    private IEnumerator NukesCoroutine()
    {
        if (nukeTriggered) yield break; // prevent multiple nukes
        nukeTriggered = true;

        yield return new WaitForSeconds(1);
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.PlaySFX("Explotion");
        yield return new WaitForSeconds(1);

        if (nukeEffectPrefab != null && playerTransform != null)
        {
            // spawn once at player’s position and orientation
            Instantiate(nukeEffectPrefab, playerTransform.position, Quaternion.identity);
        }

        if (mainCamera != null)
        {
            StartCoroutine(CameraShake(0.5f, 0.3f));
        }

        LoseGame();
    }
    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    // Getters for other systems
    public int GetCurrentBrainsRequired() => currentBrainsRequired;
    public int GetBrainsEaten() => brainsEaten;
    public int GetUpgradeLevel() => upgradeLevel;
    public float GetTimeLeft() => timeLeft;
}