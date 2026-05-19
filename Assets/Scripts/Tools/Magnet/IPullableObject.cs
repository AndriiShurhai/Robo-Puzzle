using UnityEngine;
using System.Collections;

public interface IPullableObject
{
    Transform GetTransform();
    Vector3? GetDestination();
    void PullTowardsTarget(Vector3 target);
    void ChangeToPullingState(IPullableObject controller, Vector3 finalDestination, float remaining);
    void StopPulling();
}