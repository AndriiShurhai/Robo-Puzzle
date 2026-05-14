using System.Collections;
using UnityEngine;


public interface IObjectState
{
    void Enter();
    void Update();
    void Exit();
}

public class IdleState: IObjectState
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
        timer += Time.fixedDeltaTime;
        if (timer >= idleTime)
        {
            timer = 0f;
            controller.ChangeState(new MoveState(controller));
        }
    }

    public void Exit()
    {
        // Clean up idle state if necessary
    }
}

public class MoveState : IObjectState
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

public class PushBackState : IObjectState
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

public class PullAlignState : IObjectState
{
    private IPullableObject controller;
    private Vector3 centerOfCurrentCell;
    private Vector3 finalDestination;
    private float alignSpeed = 3f;

    public PullAlignState(IPullableObject controller)
    {
        this.controller = controller;
        finalDestination = controller.GetDestination() ?? Vector3.zero;
    }
    public void Enter()
    {
        Debug.Log("Enter pull align state");
        Vector3 currentPosition = controller.GetTransform().position;
        centerOfCurrentCell = new Vector3(
            Mathf.Round(currentPosition.x),
            currentPosition.y,
            Mathf.Round(finalDestination.z)
        );
    }

    public void Update()
    {
        float step = alignSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(controller.GetTransform().position, centerOfCurrentCell);

        if (dist <= step)
        {
            float remaining = step - dist;
            controller.GetTransform().position = centerOfCurrentCell;

            controller.ChangeToPullingState(controller, finalDestination, remaining);
        }

        else
        {
            controller.GetTransform().position = Vector3.MoveTowards(controller.GetTransform().position, centerOfCurrentCell, step);
        }
    }

    public void Exit()
    {

    }

}

public class BeingPulledState : IObjectState
{
    private IPullableObject controller;
    private Vector3 targetPosition;
    private Vector3 finalDestination;
    private float moveSpeed = 5f; 
    private float carryoverBudget;
    private int cellSize = 1;

    public BeingPulledState(IPullableObject controller, Vector3 finalDestination, float leftoverMovement)
    {
        this.controller = controller;
        this.finalDestination = finalDestination;
        this.carryoverBudget = leftoverMovement;
    }

    public void Enter()
    {
        Debug.Log("Robot is being pulled!");
        SetNextTargetCell();
        controller.GetTransform().position = Vector3.MoveTowards(controller.GetTransform().position, targetPosition, carryoverBudget);
    }

    public void Update()
    {
        float step = moveSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(controller.GetTransform().position, targetPosition);

        if (dist <= step)
        {
            float remaining = step - dist;
            controller.GetTransform().position = targetPosition;

            if (IsPathBlocked())
            {
                return;
            }

            SetNextTargetCell();
            controller.GetTransform().position = Vector3.MoveTowards(controller.GetTransform().position, targetPosition, remaining);
        }
        else
        {
            controller.GetTransform().position = Vector3.MoveTowards(controller.GetTransform().position, targetPosition, step);
        }
    }

    private void SetNextTargetCell()
    {
        Vector3 directionToDestination = (finalDestination - controller.GetTransform().position).normalized;
        // Snap direction to integer grid movements
        directionToDestination = new Vector3(Mathf.Round(directionToDestination.x), 0, Mathf.Round(directionToDestination.z));
        targetPosition = controller.GetTransform().position + directionToDestination;
    }

    public void Exit() { }

    private bool IsPathBlocked()
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Walls");

        float checkDistance = cellSize * 1.5f;

        Vector3 rayOrigin = controller.GetTransform().position + (Vector3.up * 0.5f);

        if (Physics.Raycast(rayOrigin, controller.GetTransform().forward, out hit, checkDistance, layerMask))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
}

public class Robot : MonoBehaviour, IPullableObject
{
    private IObjectState currentState;
    private Vector3 pullDestination;

    void Start()
    {
        ChangeState(new IdleState(this));
    }

    void FixedUpdate()
    {
        Debug.Log("Current Robot State: " + currentState.GetType().Name);
        currentState?.Update();
    }

    public void ChangeState(IObjectState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void PullTowardsTarget(Vector3 target)
    {
        if (pullDestination == target) return; // Already pulling towards this target
        pullDestination = target;
        ChangeState(new PullAlignState(this));
    }

    public Vector3? GetDestination()
    {
        return pullDestination;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void StopPulling()
    {
        if (!(currentState is BeingPulledState))
        {
            Debug.Log("Current state is not BeingPulledState, ignoring stop pull command");
            return;
        }
        pullDestination = Vector3.zero;
        ChangeState(new IdleState(this));
    }

    public void ChangeToPullingState(IPullableObject controller, Vector3 finalDestination, float remaining)
    {
        ChangeState(new BeingPulledState(controller, finalDestination, remaining));
    }
}