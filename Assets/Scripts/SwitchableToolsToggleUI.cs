using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SwitchableToolsToggleUI : MonoBehaviour, IGameSystem
{
    [SerializeField] private GameObject toggleContainer;
    [SerializeField] private GameObject togglePrefab;

    private List<string> existingToggles = new List<string>();

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
        HideToggles();
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
        existingToggles.Clear();
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
        Debug.Log($"[SwitchableToolsToggleUI] Tool placed: {tool.displayName}");


        ISwitchableTool switchTool = toolInstance.GetComponent<ISwitchableTool>();

        if (switchTool != null)
        {
            Debug.Log("[SwitchableToolsToggleUI] Tool is switchable, creating toggle.");

            existingToggles.Add(tool.displayName);

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
                Debug.LogWarning($"[SwitchableToolsToggleUI] The prefab {togglePrefab.name} is missing a Button component.");
            }
        }
    }
}