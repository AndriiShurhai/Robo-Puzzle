using UnityEngine;
using System.Collections;

/// <summary>
/// Pure static math for 1x1x1 grid snapping.
/// No MonoBehaviour, no Unity object references — fully unit-testable.
///
/// GRID CONVENTION:
///   Cell (i, j, k) occupies the axis-aligned box [i, i+1) x [j, j+1) x [k, k+1).
///   Its center is at (i+0.5, j+0.5, k+0.5).
///   Grid colliders are placed at integer boundaries.
///
/// THE SNAPPING PROBLEM:
///   A raycast hitting a wall face lands exactly on the integer boundary between
///   the wall cell and the air cell. We want the air cell (where the tool goes),
///   not the wall cell.
///
///   Solution: step 0.5 units along the surface normal before flooring.
///   This nudges the sample point from the boundary into the center of the air cell.
///
///   Example — wall at x=3, normal = (-1, 0, 0):
///     hit.point  = (3.0, 1.2, 2.7)
///     inside     = (3.0 + (-1)*0.5,  ...) = (2.5, 1.2, 2.7)
///     FloorToInt = (2, 1, 2)
///     CellCenter = (2.5, 1.5, 2.5)  ← correct air cell in front of the wall
/// </summary>
/// 
public class GridSnapper : MonoBehaviour
{
    public static Vector3Int WorldToCell(Vector3 hitPoint, Vector3 normal)
    {
        Vector3 inside = hitPoint + normal * 0.5f;

        return new Vector3Int(
            Mathf.FloorToInt(inside.x), 
            Mathf.FloorToInt(inside.y), 
            Mathf.FloorToInt(inside.z));
    }

    public static Vector3 CellCenter(Vector3Int cell)
    {
        return new Vector3(
            cell.x + 0.5f, 
            cell.y + 0.5f, 
            cell.z + 0.5f);
    }

    public static SurfaceType ClassifySurface (Vector3 normal)
    {
        float dot = Vector3.Dot(normal.normalized, Vector3.up);
        if (dot > 0.7f) return SurfaceType.Floor;
        if (dot < -0.7f) return SurfaceType.Ceiling;
        return SurfaceType.Wall;
    }

    public static Quaternion SurfaceRotation(Vector3 normal)
    {
        Vector3 upRef = Mathf.Abs(Vector3.Dot(normal.normalized, Vector3.up)) > 0.99f
            ? Vector3.forward
            : Vector3.up;

        return Quaternion.LookRotation(normal, upRef);
    }
}