using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwitchableToolsToggleUI : MonoBehaviour
{
    [SerializeField] private GameObject toggleContainer;
    [SerializeField] private GameObject togglePrefab;

    // Keep track of tools we've already created toggles for to prevent duplicates
    private List<string> existingToggles = new List<string>();

    private void Start()
    {
        if (toggleContainer == null || togglePrefab == null)
        {
            Debug.LogError("Toggle container or prefab not assigned in SwitchableToolsToggleUI.");
            return;
        }

        // Subscribe to the event
        if (ToolPlacementSystem.Instance != null)
        {
            ToolPlacementSystem.Instance.OnToolPlaced += ToolPlacementSystem_OnToolPlaced;
        }
    }

    private void OnDestroy()
    {
        // ALWAYS unsubscribe to prevent memory leaks and missing reference exceptions
        if (ToolPlacementSystem.Instance != null)
        {
            ToolPlacementSystem.Instance.OnToolPlaced -= ToolPlacementSystem_OnToolPlaced;
        }
    }

    // NOTE: I added 'GameObject toolInstance' to the parameters. 
    // You MUST get the component from the instance in the scene, not the prefab!
    private void ToolPlacementSystem_OnToolPlaced(ToolDefinition tool, GameObject toolInstance)
    {
        Debug.Log($"[SwitchableToolsToggleUI] Tool placed: {tool.displayName}");

        // Check if a toggle for this tool already exists using our HashSet

        // Check the INSTANCE for the interface, not the prefab
        ISwitchableTool switchTool = toolInstance.GetComponent<ISwitchableTool>();

        if (switchTool != null)
        {
            Debug.Log("[SwitchableToolsToggleUI] Tool is switchable, creating toggle.");

            // Mark this tool as having a toggle
            existingToggles.Add(tool.displayName);

            // Instantiate a new toggle for this tool
            GameObject toggleObj = Instantiate(togglePrefab, toggleContainer.transform);
            toggleObj.name = tool.displayName;

            Button toggleComponent = toggleObj.GetComponent<Button>();

            if (toggleComponent != null)
            {
                toggleComponent.onClick.AddListener(() =>
                {
                    // Adding a null check in case the tool gets destroyed in the scene 
                    // while the UI button still exists
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