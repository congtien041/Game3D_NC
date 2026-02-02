using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectMapUI : MonoBehaviour
{
    public void LoadLapRacing()
    {
        SceneManager.LoadScene("LapRacing");
    }

    public void LoadMapRacing1()
    {
        SceneManager.LoadScene("MapRacing1");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
