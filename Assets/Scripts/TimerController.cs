using System;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TimerController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float initialTimeInSeconds = 60f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText;

    public event Action onTimerEnd;

    private CountdownTimer _timer;

    private void Awake()
    {
        _timer = new CountdownTimer(initialTimeInSeconds);

        _timer.OnTicked += UpdateUI;
        _timer.OnCompleted += HandleTimerEnd;
    }

    private void Start()
    {
        _timer.Play();
    }

    private void Update()
    {
        _timer.Tick(Time.deltaTime);
    }

    private void UpdateUI(float timeRemaining)
    {
        if (timeText == null) return;

        float displayTime = timeRemaining + 1f;

        int minutes = Mathf.FloorToInt(displayTime / 60);
        int seconds = Mathf.FloorToInt(displayTime % 60);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void HandleTimerEnd()
    {
        if (timeText != null) timeText.text = "00:00";
        onTimerEnd?.Invoke();
    }

    private void OnDestroy()
    {
        if (_timer != null)
        {
            _timer.OnTicked -= UpdateUI;
            _timer.OnCompleted -= HandleTimerEnd;
        }
    }
}