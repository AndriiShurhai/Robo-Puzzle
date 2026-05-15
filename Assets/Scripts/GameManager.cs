using System;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Exploring, Planning, Executing }

    [SerializeField] private Finish finishTrigger;
    [SerializeField] private TimerController timerController;

    [SerializeField] private Robot player;

    private GameState currentState;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        currentState = GameState.Exploring;
    }

    private void Start()
    {
        finishTrigger.OnLevelComplete += OnLevelComplete;
        timerController.onTimerEnd += OnLevelFailed;

        player.StartMoving();
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