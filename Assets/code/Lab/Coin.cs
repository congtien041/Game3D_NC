using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameDataManager.instance.totalCoins += coinValue;
            
            Debug.Log("Số coin hiện tại: " + GameDataManager.instance.totalCoins);

            Destroy(gameObject);
        }
    }
}