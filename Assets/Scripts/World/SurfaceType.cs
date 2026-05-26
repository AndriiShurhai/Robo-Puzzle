/// <summary>
/// Surface classification based on the hit normal's relationship to world-up.
/// Used by PlacementRules to restrict where tools can be placed.
/// </summary>
public enum SurfaceType
{
    None,
    Floor,    // normal pointing up    (dot with Vector3.up > 0.7)
    Wall,     // normal pointing horizontal
    Ceiling   // normal pointing down  (dot with Vector3.up < -0.7)
}