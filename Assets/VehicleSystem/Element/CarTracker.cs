using UnityEngine;
using System;

namespace VehicleSystem.Element
{
    public class CarTracker : MonoBehaviour
    {
        public bool isPlayer = true; 
        public int totalCheckpoints = 5; 
        public int currentLap { get; private set; } = 1;
        private int lastCheckpointIndex = -1; 
        public static event Action<CarTracker, int> OnLapCompleted;
        public static event Action<float> OnTimeBonus;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Checkpoint cp))
            {
                if (cp.isFinishLine)
                {
                    if (lastCheckpointIndex == totalCheckpoints - 1)
                    {
                        currentLap++;
                        lastCheckpointIndex = -1; 
                        
                        Debug.Log($"<color=cyan>Xe {gameObject.name} hoàn thành vòng {currentLap - 1}!</color>");
                        OnLapCompleted?.Invoke(this, currentLap);
                    }
                }
                else
                {
                    if (cp.index == lastCheckpointIndex + 1)
                    {
                        lastCheckpointIndex = cp.index; 
                        if (cp.isTimeBonus && isPlayer) 
                        {
                            OnTimeBonus?.Invoke(cp.timeBonusAmount);
                        }
                    }
                }
            }
        }
    }
}