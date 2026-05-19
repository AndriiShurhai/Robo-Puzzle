using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwitchableToolsToggleUI : MonoBehaviour
{
    [SerializeField] private GameObject toggleContainer;
    [SerializeField] private GameObject togglePrefab;

    private List<string> existingToggles = new List<string>();

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

    private void OnDestroy()
    {
        if (ToolPlacementSystem.Instance != null)
        {
            ToolPlacementSystem.Instance.OnToolPlaced -= OnToolPlaced;
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
                    if (toolInstance != null)
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