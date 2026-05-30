using UnityEngine;
using System;
using System.Collections.Generic;

public class ToolsManager : MonoBehaviour, IGameSystem
{
    public event Action<ToolDefinition, int> OnInventoryChanged;

    public static ToolsManager Instance { get; private set; }

    private List<GameObject> _tools;
    private IGameEvents _gameEvents;

    private Dictionary<ToolDefinition, int> _initialLoadout = new Dictionary<ToolDefinition, int>();
    private Dictionary<ToolDefinition, int> _currentInventory = new Dictionary<ToolDefinition, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _tools = new List<GameObject>();
    }
    private void Start()
    {
        ToolPlacementSystem.Instance.OnToolPlaced += OnToolPlaced;
    }

    public void Initialize(IGameEvents gameEvents)
    {
        _gameEvents = gameEvents;
        _gameEvents.OnExploreEntered += OnExplore;
        _gameEvents.OnPlanEntered += OnPlan;
        _gameEvents.OnExecuteEntered += OnExecute;
    }

    public void SetupLevelLoadout(List<ToolLoadout> levelTools)
    {
        _initialLoadout.Clear();
        _currentInventory.Clear();

        foreach (var item in levelTools)
        {
            _initialLoadout[item.tool] = item.startingAmount;
            _currentInventory[item.tool] = item.startingAmount;
        }
    }

    private void OnDestroy()
    {
        if (ToolPlacementSystem.Instance != null) ToolPlacementSystem.Instance.OnToolPlaced -= OnToolPlaced;
        if (_gameEvents == null) return;
        _gameEvents.OnExploreEntered -= OnExplore;
        _gameEvents.OnPlanEntered -= OnPlan;
        _gameEvents.OnExecuteEntered -= OnExecute;
    }

    private void OnExplore()
    {
        ClearBoardAndRefund();
    }

    private void OnPlan()
    {
        ClearBoardAndRefund();
    }

    private void OnExecute()
    {

    }

    private void ClearBoardAndRefund()
    {
        // Destroy the physical objects
        foreach (GameObject go in _tools)
        {
            Destroy(go);
        }
        _tools.Clear();

        // Refund the inventory back to the initial level amounts
        foreach (var kvp in _initialLoadout)
        {
            _currentInventory[kvp.Key] = kvp.Value;
            OnInventoryChanged?.Invoke(kvp.Key, kvp.Value);
        }
    }

    private void OnToolPlaced(ToolDefinition toolDef, GameObject toolInstance)
    {
        _tools.Add(toolInstance);

        if (_currentInventory.ContainsKey(toolDef))
        {
            _currentInventory[toolDef]--;
            OnInventoryChanged?.Invoke(toolDef, _currentInventory[toolDef]);
        }
    }
}
