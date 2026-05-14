using System;
using UnityEngine;

public class Finish : MonoBehaviour
{
    public event Action OnLevelComplete;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            OnLevelComplete?.Invoke();
        }
    }
}
