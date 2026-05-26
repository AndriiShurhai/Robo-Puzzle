using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(menuName = "Puzzle/Placement Rules/Surface Rule", fileName = "Rule_Surface_")]

public class SurfacePlacementRule : PlacementRule
{
    [SerializeField] private SurfaceType[] validSurfaces;

    public override bool Evaluate(PlacementContext context)
    {
        return Array.IndexOf(validSurfaces, context.Surface) >= 0;
    }

    public override string InvalidReason
    {
        get
        {
            if (validSurfaces == null || validSurfaces.Length == 0) return "No valid surfaces defined for this tool.";
            return $"Must place on {string.Join(", ", validSurfaces)}.";
        }
    }
}