using UnityEngine;

public interface IDirectable
{
    void SetDirection(Vector3 direction, Transform platform);
}
public class DirectionPlatform : MonoBehaviour, IRotateable
{
    private bool hasLeft = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasLeft) return;
        IDirectable dir = other.GetComponent<IDirectable>();
        dir?.SetDirection(transform.forward, transform);
        hasLeft = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasLeft = true;
        }
    }
}
