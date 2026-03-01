using UnityEngine;
using System;
using System.Collections;

namespace VehicleSystem.Managers
{
    public enum GameMode { Circuit, TimeAttack, FreeRoam }
    public enum GameState { Loading, Countdown, Racing, Finished }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("--- GAME CONFIGURATION ---")]
        public GameMode currentMode = GameMode.Circuit;
        
        public GameState State { get; private set; }
        public float totalRaceTime { get; private set; } // Đồng hồ tổng

        // Các sự kiện phát loa
        public static event Action<GameState> OnStateChanged;
        public static event Action<string> OnCountdownUpdate;
        public static event Action OnRaceStart;
        public static event Action OnRaceFinished;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            ChangeState(GameState.Countdown);
        }

        private void Update()
        {
            // Chỉ đếm thời gian khi đang đua
            if (State == GameState.Racing)
            {
                totalRaceTime += Time.deltaTime;
            }
        }

        public void ChangeState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Countdown:
                    StartCoroutine(CountdownRoutine());
                    break;
                case GameState.Racing:
                    totalRaceTime = 0f;
                    OnRaceStart?.Invoke(); 
                    break;
                case GameState.Finished:
                    OnRaceFinished?.Invoke(); 
                    Debug.Log($"<color=green>🏁 KẾT THÚC GAME! Thời gian: {totalRaceTime:F2}s</color>");
                    break;
            }
        }

        private IEnumerator CountdownRoutine()
        {
            OnCountdownUpdate?.Invoke(""); 
            yield return new WaitForSeconds(1f);
            OnCountdownUpdate?.Invoke("3");
            yield return new WaitForSeconds(1f);
            OnCountdownUpdate?.Invoke("2");
            yield return new WaitForSeconds(1f);
            OnCountdownUpdate?.Invoke("1");
            yield return new WaitForSeconds(1f);
            OnCountdownUpdate?.Invoke("GO!");
            
            ChangeState(GameState.Racing);

            yield return new WaitForSeconds(1f);
            OnCountdownUpdate?.Invoke(""); 
        }
    }
}