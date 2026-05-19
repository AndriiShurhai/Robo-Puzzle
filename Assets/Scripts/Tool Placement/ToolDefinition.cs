using UnityEngine;


[CreateAssetMenu(menuName = "Puzzle/ToolDefinition", fileName = "Tool_")]

public class ToolDefinition : ScriptableObject
{
    [Header("Identify")]
    public string displayName;
    public Sprite icon;

    [Header("Prefabs")]
    [Tooltip("The real GameObject instantiated on valid placement")]
    public GameObject toolPrefab;

    [Tooltip("Override ghost visual. Leave null to auto-build ghos from toolPrefab")]
    public GameObject ghostPrefabOverride;

    [Header("Placement Rules")]
    [Tooltip("ALL rules must pass for placement to be valid. Order doesn't matter.")]
    public PlacementRule[] placementRules;

    public bool IsValidPlacement(PlacementContext context)
    {
        foreach (PlacementRule rule in placementRules)
        {
            if (!rule.Evaluate(context)) return false;
        }
        return true;
    }

    public string GetFirstInvalidReason(PlacementContext context)
    {
        foreach (PlacementRule rule in placementRules)
        {
            if (!rule.Evaluate(context)) return rule.InvalidReason;
        }
        return null;
    }

    public GameObject GetGhostPrefab()
    {
        if (ghostPrefabOverride != null) return ghostPrefabOverride;

        return toolPrefab;
    }
}