using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void PlayGame()
{
    SceneManager.LoadScene("SelectMap");
}


    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
