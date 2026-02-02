using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGameUI : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject minimapUI;
    public GameObject gameHUD;   // GameController

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        minimapUI.SetActive(true);
        gameHUD.SetActive(true);
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0 : 1;
        pausePanel.SetActive(isPaused);

        // ❗ Ẩn toàn bộ HUD khi pause
        minimapUI.SetActive(!isPaused);
        gameHUD.SetActive(!isPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1;
        pausePanel.SetActive(false);
        minimapUI.SetActive(true);
        gameHUD.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
