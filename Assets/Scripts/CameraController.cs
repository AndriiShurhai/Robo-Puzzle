using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;


    private Vector3 offset;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        offset = transform.position - player.transform.position;
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }
}
