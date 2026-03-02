using UnityEngine;
using UnityEngine.SceneManagement;

namespace VehicleSystem.UI
{
    public class LoadScene : MonoBehaviour
    {
        public void Load()
        {
            SceneManager.LoadScene("Main");
        }
    }
}