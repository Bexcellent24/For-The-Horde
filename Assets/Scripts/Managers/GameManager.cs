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

    [Header("Game Settings")]
    [SerializeField] private float gameTime = 300f;

    private float timeLeft;
    private int brainsEaten = 0;
    private bool gameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        timeLeft = gameTime;
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
    }

    private void UpdateUI()
    {
        timerText.text = Mathf.Ceil(timeLeft).ToString();
        brainText.text = "Brains Eaten : " + brainsEaten;
    }

    public void WinGame()
    {
        gameOver = true;
        Debug.Log("You Win!");
        // Add win screen
    }

    public void LoseGame()
    {
        gameOver = true;
        Debug.Log("You Lose!");
        // Add lose screen
    }
}
