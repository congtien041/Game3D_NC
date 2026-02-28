using UnityEngine;
using VehicleSystem.Element;

namespace VehicleSystem.Managers
{
    public class RaceModeManager : MonoBehaviour
    {
        [Header("CIRCUIT")]
        public int totalLaps = 3;

        [Header("TIME ATTACK")]
        public float startingTime = 60f;
        private float timeLeft;

        [Header("ELIMINATION")]
        public float eliminationInterval = 20f;
        private float eliminationTimer;
        private GameMode currentCachedMode;

        private void OnEnable()
        {
            CarTracker.OnLapCompleted += HandleLapCompleted;
            CarTracker.OnTimeBonus += AddTime;
        }

        private void OnDisable()
        {
            CarTracker.OnLapCompleted -= HandleLapCompleted;
            CarTracker.OnTimeBonus -= AddTime;
        }

        private void Start()
        {
            timeLeft = startingTime;
            eliminationTimer = eliminationInterval;

            // Cache mode ngay từ đầu
            if (GameManager.Instance != null)
            {
                currentCachedMode = GameManager.Instance.currentMode;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Racing) 
                return;

            if (currentCachedMode == GameMode.TimeAttack)
            {
                HandleTimeAttackMode();
            }
            else if (currentCachedMode == GameMode.Elimination)
            {
                HandleEliminationMode();
            }
        }
 
        private void HandleLapCompleted(CarTracker car, int newLap)
        {
            if (!car.isPlayer) return;

            if (currentCachedMode == GameMode.Circuit)
            {
                if (newLap > totalLaps)
                {
                    GameManager.Instance.ChangeState(GameState.Finished);
                }
            }
        }

        public void AddTime(float seconds)
        {
            if (currentCachedMode == GameMode.TimeAttack)
            {
                timeLeft += seconds;
                Debug.Log($"<color=yellow>+ {seconds} Giây Bonus!</color>");
            }
        }

        private void HandleTimeAttackMode()
        {
            timeLeft -= Time.deltaTime;

            if (timeLeft <= 0)
            {
                Debug.Log("<color=red>HẾT GIỜ! BẠN ĐÃ THUA!</color>");
                GameManager.Instance.ChangeState(GameState.Finished);
            }
        }

        private void HandleEliminationMode()
        {
            eliminationTimer -= Time.deltaTime;

            if (eliminationTimer <= 0)
            {
                EliminateLastCar();
                eliminationTimer = eliminationInterval; // Reset đếm lại
            }
        }

        private void EliminateLastCar()
        {
            Debug.Log("<color=orange>MỘT XE VỪA BỊ LOẠI!</color>");
            // Logic hủy xe chót bảng sẽ viết vào đây sau
        }
    }
}