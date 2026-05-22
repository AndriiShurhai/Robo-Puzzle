using UnityEngine;
using System.Collections.Generic;

public class ToolsManager : MonoBehaviour, IGameSystem
{
    private List<GameObject> _tools;
    private IGameEvents _gameEvents;

    private void Awake()
    {
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

    private void OnDestroy()
    {
        if (ToolPlacementSystem.Instance != null) ToolPlacementSystem.Instance.OnToolPlaced -= OnToolPlaced;
    }

    private void OnExplore()
    {
        foreach (GameObject go in _tools)
        {
            Destroy(go);
        }
        _tools.Clear();
    }

    private void OnPlan()
    {
        foreach (GameObject go in _tools)
        {
            Destroy(go);
        }
        _tools.Clear();
    }

    private void OnExecute()
    {

    }

    private void OnToolPlaced(ToolDefinition toolDef, GameObject toolInstance)
    {
        if (_tools == null) _tools = new List<GameObject>();
        _tools.Add(toolInstance);
    }
}
