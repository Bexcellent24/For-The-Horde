using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject MainMenu;
    
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public void ShowControls()
    {
        controlsMenu.SetActive(true);
        MainMenu.SetActive(false);
    }
    
    public void HideControls()
    {
        controlsMenu.SetActive(false);
        MainMenu.SetActive(true);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
