using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class GameManager : MonoBehaviour, IGameEvents
{
    public static GameManager Instance { get; private set; }

    public event Action OnLevelComplete;
    public event Action OnLevelFailed;

    public event Action OnExploreEntered;
    public event Action OnPlanEntered;
    public event Action OnExecuteEntered;
    public enum GameState { Exploring, Planning, Executing }

    [SerializeField] private Button exploreStageButton;
    [SerializeField] private Button planStageButton;
    [SerializeField] private Button executeStageButton;
    [SerializeField] private Finish finishTrigger;
    [SerializeField] private TimerController timerController;
    [SerializeField] private List<MonoBehaviour> gameSystems;
   

    [SerializeField] private Robot player;

    private GameState currentState;

    public GameState CurrentState { get => currentState; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (player == null)
        {
            player = FindFirstObjectByType<Robot>();
            gameSystems.Add(player);
        }

        currentState = GameState.Exploring;

        exploreStageButton.onClick.AddListener(() => ChangeState(GameState.Exploring));
        planStageButton.onClick.AddListener(() => ChangeState(GameState.Planning));
        executeStageButton.onClick.AddListener(() => ChangeState(GameState.Executing));
    }

    private void Start()
    {
        finishTrigger.OnLevelComplete += CompleteLevel;
        timerController.onTimerEnd += FailLevel;

        foreach (var system in gameSystems)
        {
            if (system is IGameSystem gameSystem)
            {
                gameSystem.Initialize(this);
            }
        }

        OnExploreEntered?.Invoke();
    }

    private void OnDestroy()
    {
        finishTrigger.OnLevelComplete -= CompleteLevel;
        timerController.onTimerEnd -= FailLevel;
    }

    public void CompleteLevel()
    {
        Debug.Log("Level completed");
        ChangeState(GameState.Exploring);
        OnLevelComplete?.Invoke();
    }

    public void FailLevel()
    {
        Debug.Log("Level failed");
        ChangeState(GameState.Exploring);
        OnLevelFailed?.Invoke();
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;

        switch(newState)
        {
            case GameState.Exploring:
                OnExploreEntered?.Invoke();
                break;
            case GameState.Planning:
                OnPlanEntered?.Invoke();
                break;

            case GameState.Executing:
                OnExecuteEntered?.Invoke();
                break;
        }
    }
}