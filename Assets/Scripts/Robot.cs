using UnityEngine;

public class Robot : MonoBehaviour
{
    [SerializeField] private int speed;

    private bool isMoving = true;

    private void Update()
    {
        if (isMoving)
        {
            Vector3 direction = transform.forward;
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isMoving = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isMoving = true;
        }
    }

}
