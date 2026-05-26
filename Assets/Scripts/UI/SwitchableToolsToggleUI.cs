using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SwitchableToolsToggleUI : MonoBehaviour, IGameSystem
{
    [SerializeField] private GameObject toggleContainer;
    [SerializeField] private GameObject togglePrefab;

    private IGameEvents _gameEvents;

    private void Start()
    {
        if (toggleContainer == null || togglePrefab == null)
        {
            Debug.LogError("Toggle container or prefab not assigned in SwitchableToolsToggleUI.");
            return;
        }

        if (ToolPlacementSystem.Instance != null)
        {
            ToolPlacementSystem.Instance.OnToolPlaced += OnToolPlaced;
        }
    }

    public void Initialize(IGameEvents gameEvents)
    {
        this._gameEvents = gameEvents;
        gameEvents.OnExploreEntered += OnExplore;
        gameEvents.OnPlanEntered += OnPlan;
        gameEvents.OnExecuteEntered += OnExecute;
    }

    private void OnDestroy()
    {
        if (ToolPlacementSystem.Instance != null)
        {
            ToolPlacementSystem.Instance.OnToolPlaced -= OnToolPlaced;
        }
        if (_gameEvents == null) return;
        _gameEvents.OnExploreEntered -= OnExplore;
        _gameEvents.OnPlanEntered -= OnPlan;
        _gameEvents.OnExecuteEntered -= OnExecute;
        
    }

    private void OnExplore()
    {
        ClearToggles();
    }

    private void OnPlan()
    {
        ClearToggles();
    }

    private void OnExecute()
    {
    }

    private void ClearToggles()
    {
        foreach (Transform child in toggleContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void HideToggles()
    {
        foreach (Transform child in toggleContainer.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    private void OnToolPlaced(ToolDefinition tool, GameObject toolInstance)
    {

        ISwitchableTool switchTool = toolInstance.GetComponent<ISwitchableTool>();

        if (switchTool != null)
        {
            GameObject toggleObj = Instantiate(togglePrefab, toggleContainer.transform);
            toggleObj.name = tool.displayName;

            Button toggleComponent = toggleObj.GetComponent<Button>();

            if (toggleComponent != null)
            {
                toggleComponent.onClick.AddListener(() =>
                {
                    if (toolInstance != null && GameManager.Instance.CurrentState == GameManager.GameState.Executing)
                    {
                        switchTool.ToggleSwitch();
                    }
                });
            }

            else
            {
            }
        }
    }
}