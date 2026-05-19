using UnityEngine;
using System.Collections;

public abstract class PlacementRule : ScriptableObject
{
    public abstract bool Evaluate(PlacementContext context);

    public virtual string InvalidReason => "Placement rule not satisfied.";
}