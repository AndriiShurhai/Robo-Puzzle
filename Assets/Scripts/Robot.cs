using System.Collections;
using UnityEngine;


public interface IRobotState
{
    void Enter();
    void Update();
    void Exit();
}

public class IdleState: IRobotState
{
    private float idleTime = 2f;
    private float timer = 0f;
    private Robot controller;

    public IdleState(Robot controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Enter idle state");
        timer = 0f;
    }

    public void Update()
    {
        Debug.Log("The timer is " + timer);
        timer += Time.fixedDeltaTime;
        if (timer >= idleTime)
        {
            timer = 0f;
            Debug.Log("Chnage state to move");
            controller.ChangeState(new MoveState(controller));
        }
    }

    public void Exit()
    {
        // Clean up idle state if necessary
    }
}

public class MoveState : IRobotState
{
    private Robot controller;
    private float moveSpeed = 3f;
    private int cellSize = 1;
    private Vector3 targetPosition;

    public MoveState(Robot controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Enter move state");
        // Compute first target cell once
        SetNextTarget();
    }

    public void Update()
    {
        float step = moveSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(controller.transform.position, targetPosition);

        if (dist <= step)
        {
            // Use remaining budget to continue into the next cell immediately
            float remaining = step - dist;
            controller.transform.position = targetPosition;

            if (IsPathBlocked())
            {
                Debug.Log("Path is blocked, switching to push back state");
                controller.ChangeState(new PushBackState(controller));
                return;
            }

            SetNextTarget();
            controller.transform.position += controller.transform.forward * remaining;
        }
        else
        {
            controller.transform.position = Vector3.MoveTowards(
                controller.transform.position,
                targetPosition,
                step
            );
        }
    }

    public void Exit() { }

    private void SetNextTarget()
    {
        targetPosition = controller.transform.position + controller.transform.forward * cellSize;
    }

    private bool IsPathBlocked()
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Walls");

        float checkDistance = cellSize * 1.5f;

        Vector3 rayOrigin = controller.transform.position + (Vector3.up * 0.5f);

        if (Physics.Raycast(rayOrigin, controller.transform.forward, out hit, checkDistance, layerMask))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
}

public class PushBackState : IRobotState
{
    private Robot controller;
    private float pushBackDistance = 1f;
    private float pushBackSpeed = 3f;
    private Vector3 targetPosition;

    public PushBackState(Robot controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Enter push back state");

        Vector3 pushDirection = -controller.transform.forward;
        targetPosition = controller.transform.position + (pushDirection * pushBackDistance);
    }

    public void Update()
    {
        float step = pushBackSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(controller.transform.position, targetPosition);

        if (dist <= step)
        {
            controller.transform.position = targetPosition;
            controller.ChangeState(new IdleState(controller));
        }
        else
        {
            controller.transform.position = Vector3.MoveTowards(
                controller.transform.position,
                targetPosition,
                step
            );
        }
    }

    public void Exit()
    {
    }
}

public class Robot : MonoBehaviour
{
    private IRobotState currentState;

    void Start()
    {
        ChangeState(new IdleState(this));
    }

    void FixedUpdate()
    {
        currentState?.Update();
    }

    public void ChangeState(IRobotState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}