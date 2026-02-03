using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void LoadScenes(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
