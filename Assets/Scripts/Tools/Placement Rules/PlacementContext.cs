using UnityEngine;


public readonly struct PlacementContext
{
    public readonly Vector3Int Cell;

    public readonly Vector3 SnappedPosition;

    public readonly Quaternion SnappedRotation;

    public readonly Vector3 SurfaceNormal;

    public readonly SurfaceType Surface;

    public readonly RaycastHit Hit;

    public PlacementContext(
        Vector3Int cell,
        Vector3 snappedPosition,
        Quaternion snappedRotation,
        Vector3 surfaceNormal,
        SurfaceType surface,
        RaycastHit hit
    )
    {
        Cell = cell;
        SnappedPosition = snappedPosition;
        SnappedRotation = snappedRotation;
        SurfaceNormal = surfaceNormal;
        Surface = surface;
        Hit = hit;
    }
}