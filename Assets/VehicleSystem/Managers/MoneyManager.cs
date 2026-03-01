using System;
using System.Text;
using UnityEngine;

namespace VehicleSystem.Managers
{
    public class MoneyManager : MonoBehaviour
    {
        public static MoneyManager Instance { get; private set; }
        public event Action<int> OnMoneyChanged;
        private ObfuscatedInt currentMoney;
        private readonly string hashKey = "Hyper-D-Racing"; 
        private const string SAVE_KEY = "EncryptedPlayerFunds";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                currentMoney = new ObfuscatedInt(0);
                LoadMoney();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public int GetBalance()
        {
            return currentMoney.GetValue();
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;
            
            int newBalance = currentMoney.GetValue() + amount;
            currentMoney.SetValue(newBalance);
            
            SaveMoney();
            OnMoneyChanged?.Invoke(newBalance);
        }

        public bool SpendMoney(int amount)
        {
            if (amount <= 0) return false;
            
            int balance = currentMoney.GetValue();

            if (balance >= amount)
            {
                int newBalance = balance - amount;
                currentMoney.SetValue(newBalance);
                
                SaveMoney();
                OnMoneyChanged?.Invoke(newBalance);
                return true; 
            }
            
            return false;
        }

        private void SaveMoney()
        {
            string encrypted = Encrypt(currentMoney.GetValue().ToString());
            PlayerPrefs.SetString(SAVE_KEY, encrypted);
            PlayerPrefs.Save();
        }

        private void LoadMoney()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    string encrypted = PlayerPrefs.GetString(SAVE_KEY);
                    string decrypted = Decrypt(encrypted);
                    if (int.TryParse(decrypted, out int savedMoney))
                    {
                        currentMoney.SetValue(savedMoney);
                        return;
                    }
                }
                catch
                {
                    Debug.LogWarning("Save file tampered! Resetting money to 0.");
                }
            }
            currentMoney.SetValue(0);
        }

        private string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(hashKey);
            
            for (int i = 0; i < plainTextBytes.Length; i++)
            {
                plainTextBytes[i] ^= keyBytes[i % keyBytes.Length];
            }
            return Convert.ToBase64String(plainTextBytes);
        }

        private string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(hashKey);
            
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                encryptedBytes[i] ^= keyBytes[i % keyBytes.Length];
            }
            return Encoding.UTF8.GetString(encryptedBytes);
        }
    }

    public struct ObfuscatedInt
    {
        private int hiddenValue;
        private int currentKey;

        public ObfuscatedInt(int initialValue)
        {
            currentKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            hiddenValue = initialValue ^ currentKey;
        }

        public int GetValue()
        {
            return hiddenValue ^ currentKey;
        }

        public void SetValue(int value)
        {
            currentKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            hiddenValue = value ^ currentKey;
        }
    }
}