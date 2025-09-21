using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private GameObject pauseMenu;

    [Header("Animation Settings")]
    [SerializeField] private float zoomDuration = 1f; // how long it takes to zoom
    [SerializeField] private float maxScale = 2f; // final scale of text
    [SerializeField] private float delayBeforeMenu = 3f; // time to wait before going to main menu

    private void OnEnable()
    {
        GameManager.OnGameLost += GameLostHandler;
        GameManager.OnGameWon += GameWonHandler;
    }

    private void OnDisable()
    {
        GameManager.OnGameLost -= GameLostHandler;
        GameManager.OnGameWon -= GameWonHandler;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    private void GameLostHandler()
    {
        gameOverMenu.SetActive(true);

        gameOverText.text = "Game Over";
        
        // Reset text scale before animating
        gameOverText.transform.localScale = Vector3.zero;

        // Start the zoom animation
        StartCoroutine(ZoomText());
    }
    
    private void GameWonHandler()
    {
        gameOverMenu.SetActive(true);

        gameOverText.text = "For The Horde!";
        
        // Reset text scale before animating
        gameOverText.transform.localScale = Vector3.zero;

        // Start the zoom animation
        StartCoroutine(ZoomText());
    }

    private IEnumerator ZoomText()
    {
        float t = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * maxScale;

        // Smoothly scale text
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float progress = t / zoomDuration;
            progress = Mathf.SmoothStep(0f, 1f, progress); // makes the scaling ease in/out
            gameOverText.transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        gameOverText.transform.localScale = targetScale;

        // Wait a bit before switching to main menu
        yield return new WaitForSeconds(delayBeforeMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        AudioManager.Instance?.StopMusic();
        pauseMenu.SetActive(true);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayMusic("Background");
        pauseMenu.SetActive(false);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}