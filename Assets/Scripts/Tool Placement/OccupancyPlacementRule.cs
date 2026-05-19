using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Puzzle/Placement Rules/Occupancy Rule", fileName = "Rule_Occupancy_")]

public class OccupancyPlacementRule : PlacementRule
{
    public override bool Evaluate(PlacementContext context)
    {
        if (ToolPlacementSystem.Instance == null) return false;

        return !ToolPlacementSystem.Instance.IsCellOccuped(context.Cell);
    }
    public override string InvalidReason => "Space is occupied.";
}