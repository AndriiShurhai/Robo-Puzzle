using System;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Finish finishTrigger;

    [SerializeField] private TimerController timerController;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        finishTrigger.OnLevelComplete += OnLevelComplete;
        timerController.onTimerEnd += OnLevelFailed;
    }

    private void OnLevelComplete()
    {
        Debug.Log("Level completed");
    }

    private void OnLevelFailed()
    {
        Debug.Log("Level failed");
    }
}