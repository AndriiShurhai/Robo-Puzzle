using UnityEngine;

public class KillPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.FailLevel();
        }
    }
}
