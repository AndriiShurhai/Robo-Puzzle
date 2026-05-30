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
public static class GridSnapper
{
    public static Vector3Int WorldToCell(Vector3 hitPoint, Vector3 normal)
    {
        Vector3 inside = hitPoint + normal * 0.5f;

        return new Vector3Int(
            Mathf.FloorToInt(inside.x), 
            Mathf.FloorToInt(inside.y), 
            Mathf.FloorToInt(inside.z));
    }

    public static Vector3Int WorldToCell(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x),
            Mathf.FloorToInt(pos.y),
            Mathf.FloorToInt(pos.z));
    }

    public static Vector3 CellCenter(Vector3Int cell)
    {
        return new Vector3(
            cell.x + 0.5f, 
            cell.y + 0.5f, 
            cell.z + 0.5f);
    }

    public static bool IsCellCenter(Vector3 cell)
    {
        return Mathf.Approximately(cell.x % 1.0f, 0.5f) &&
               Mathf.Approximately(cell.y % 1.0f, 0.5f) &&
               Mathf.Approximately(cell.z % 1.0f, 0.5f);
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
        SurfaceType surface = ClassifySurface(normal);
        if (surface == SurfaceType.Floor || surface == SurfaceType.Ceiling)
        {
            // Normal becomes the object's UP — it lies flat on the surface.
            // Z points world forward as a sensible default (player rotates from here).
            return Quaternion.LookRotation(Vector3.forward, normal);
        }
        else
        {
            // Normal becomes the object's FORWARD — it faces outward from the wall.
            return Quaternion.LookRotation(normal, Vector3.up);
        }
    }
}