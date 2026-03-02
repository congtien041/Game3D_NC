using UnityEngine;
using TMPro;
using VehicleSystem.Managers;

public class RaceTimeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private void OnEnable()
    {
        GameManager.OnRaceStart += ResetTime;
        GameManager.OnRaceFinished += StopUpdating;
    }

    private void OnDisable()
    {
        GameManager.OnRaceStart -= ResetTime;
        GameManager.OnRaceFinished -= StopUpdating;
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.State == GameState.Racing)
        {
            float time = GameManager.Instance.totalRaceTime;
            timeText.text = FormatTime(time);
        }
    }

    private void ResetTime()
    {
        timeText.text = "00:00.00";
    }

    private void StopUpdating()
    {
        float finalTime = GameManager.Instance.totalRaceTime;
        timeText.text = FormatTime(finalTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}