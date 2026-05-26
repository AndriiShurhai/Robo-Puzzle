using UnityEngine;

public class DirectionPlatform : MonoBehaviour, IRotateable
{
    private bool hasLeft = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasLeft) return;
        IDirectable dir = other.GetComponent<IDirectable>();
        dir?.SetDirection(transform.forward, transform);
        hasLeft = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasLeft = true;
        }
    }
}
