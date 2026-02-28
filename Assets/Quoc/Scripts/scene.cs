using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene: MonoBehaviour
{
    public string gameScene;   

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameScene);
    }
}