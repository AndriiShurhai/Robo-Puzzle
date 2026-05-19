using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;



public class IdleState: IState
{
    private Robot _controller;

    public IdleState(Robot controller)
    {
        this._controller = controller;
    }

    public void Enter() { }

    public void Update() { }

    public void Exit() { }
}

public class MoveState : IState
{
    private Robot _controller;
    private Vector3 _targetPosition;

    public MoveState(Robot controller)
    {
        this._controller = controller;
    }

    public void Enter()
    {
        SetNextTarget();
    }

    public void Update()
    {
        float step = _controller.MoveSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(_controller.transform.position, _targetPosition);

        if (dist <= step)
        {
            // Use remaining budget to continue into the next cell immediately
            float remaining = step - dist;
            _controller.transform.position = _targetPosition;

            if (Robot.IsPathBlocked(_controller.transform, _controller.CellSize * 1.5f))
            {
                _controller.ChangeState(new PushBackState(_controller));
                return;
            }

            SetNextTarget();
            _controller.transform.position += _controller.transform.forward * remaining;
        }
        else
        {
            _controller.transform.position = Vector3.MoveTowards(
                _controller.transform.position,
                _targetPosition,
                step
            );
        }
    }

    public void Exit() { }

    private void SetNextTarget()
    {
        _targetPosition = _controller.transform.position + _controller.transform.forward * _controller.CellSize;
    }
}

public class PushBackState : IState
{
    private Robot _controller;

    private Vector3 _targetPosition;

    private CountdownTimer _timer;

    public PushBackState(Robot controller)
    {
        this._controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Enter push back state");

        _timer = new CountdownTimer(1f);
        Vector3 pushDirection = -_controller.transform.forward;
        _targetPosition = _controller.transform.position + (pushDirection * _controller.PushBackDistance);
        _timer.OnCompleted += () => _controller.ChangeState(new MoveState(_controller));
        _timer.Play(1f);
    }

    public void Update()
    {
        float step = _controller.PushBackSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(_controller.transform.position, _targetPosition);

        if (dist <= step)
        {
            _controller.transform.position = _targetPosition;
            _timer.Tick(Time.deltaTime);
        }
        else
        {
            _controller.transform.position = Vector3.MoveTowards(
                _controller.transform.position,
                _targetPosition,
                step
            );
        }
    }

    public void Exit() { }
}

public class PullAlignState : IState
{
    private IPullableObject _controller;
    private float _alignSpeed;
    private Vector3 _finalDestination;
    private Vector3 _alignTarget;

    public PullAlignState(IPullableObject controller, float alignSpeed)
    {
        this._controller = controller;
        this._alignSpeed = alignSpeed;
        _finalDestination = controller.GetDestination() ?? Vector3.zero;
    }
    public void Enter()
    {
        Debug.Log("Enter pull align state");
        Vector3 currentPos = _controller.GetTransform().position;

        float distX = Mathf.Abs(_finalDestination.x - currentPos.x);
        float distZ = Mathf.Abs(_finalDestination.z - currentPos.z);

        
        if (distX > distZ)
        {
            _alignTarget = new Vector3(
                Mathf.Round(currentPos.x), // Stay in current X column (snapped to grid)
                currentPos.y,              // Keep height
                _finalDestination.z         // Align exactly with magnet's Z row
            );
        }
        else 
        {
            _alignTarget = new Vector3(
                _finalDestination.x,        // Align exactly with magnet's X column
                currentPos.y,              // Keep height
                Mathf.Round(currentPos.z)  // Stay in current Z row (snapped to grid)
            );
        }
    }

    public void Update()
    {
        Transform controllerTransform = _controller.GetTransform();
        float step = _alignSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(controllerTransform.position, _alignTarget);

        if (dist <= step)
        {
            float remaining = step - dist;
            controllerTransform.position = _alignTarget;
            _controller.ChangeToPullingState(_controller, _finalDestination, remaining);
        }

        else
        {
            controllerTransform.position = Vector3.MoveTowards(controllerTransform.position, _alignTarget, step);
        }
    }

    public void Exit() { }
}

public class BeingPulledState : IState
{
    private IPullableObject _controller;
    private float _moveSpeed = 5f; 
    private float _carryoverBudget;
    private int _cellSize = 1;
    private Vector3 _targetPosition;
    private Vector3 _finalDestination;

    public BeingPulledState(
        IPullableObject controller, 
        Vector3 finalDestination, 
        float leftoverMovement,
        float moveSpeed,
        float cellSize)
    {
        this._controller = controller;
        this._finalDestination = finalDestination;
        this._carryoverBudget = leftoverMovement;
        this._moveSpeed = moveSpeed;
        this._cellSize = (int)cellSize;
    }

    public void Enter()
    {
        SetNextTargetCell();
        _controller.GetTransform().position = Vector3.MoveTowards(_controller.GetTransform().position, _targetPosition, _carryoverBudget);
    }

    public void Update()
    {
        Transform controllerTransform = _controller.GetTransform();
        float step = _moveSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(controllerTransform.position, _targetPosition);

        if (dist <= step)
        {
            float remaining = step - dist;
            controllerTransform.position = _targetPosition;

            if (Robot.IsPathBlocked(controllerTransform, _cellSize * 1.5f))
            {
                return;
            }

            SetNextTargetCell();
            controllerTransform.position = Vector3.MoveTowards(controllerTransform.position, _targetPosition, remaining);
        }
        else
        {
            controllerTransform.position = Vector3.MoveTowards(controllerTransform.position, _targetPosition, step);
        }
    }

    private void SetNextTargetCell()
    {
        Vector3 directionToDestination = (_finalDestination - _controller.GetTransform().position).normalized;
        // Snap direction to integer grid movements
        directionToDestination = new Vector3(Mathf.Round(directionToDestination.x), 0, Mathf.Round(directionToDestination.z));
        _targetPosition = _controller.GetTransform().position + directionToDestination;
    }

    public void Exit() { }
}

public class Robot : MonoBehaviour, IPullableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int cellSize = 1;

    [Header("Push Back")]
    [SerializeField] private float pushBackDistance = 1f;
    [SerializeField] private float pushBackSpeed = 3f;

    [Header("Being Pulled")]
    [SerializeField] private float alignSpeed = 3f;
    [SerializeField] private float pulledMoveSpeed = 5f;


    public float MoveSpeed => moveSpeed;
    public int CellSize => cellSize;
    public float PushBackDistance => pushBackDistance;
    public float PushBackSpeed => pushBackSpeed;
    public float AlignSpeed => alignSpeed;
    public float PulledMoveSpeed => pulledMoveSpeed;

    private IState _currentState;
    private Vector3 _pullDestination;

    private void Awake()
    {
        ChangeState(new IdleState(this));
    }
    void Start()
    {
    }

    void FixedUpdate()
    {
        Debug.Log("Current Robot State: " + _currentState.GetType().Name);
        _currentState?.Update();
    }

    public void ChangeState(IState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void PullTowardsTarget(Vector3 target)
    {
        if (_pullDestination == target) return; // Already pulling towards this target
        _pullDestination = target;
        ChangeState(new PullAlignState(this, alignSpeed));
    }

    public Vector3? GetDestination()
    {
        return _pullDestination;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void StartMoving()
    {
        ChangeState(new MoveState(this));
    }

    public void StopPulling()
    {
        if (!(_currentState is BeingPulledState))
        {
            Debug.Log("Current state is not BeingPulledState, ignoring stop pull command");
            return;
        }
        _pullDestination = Vector3.zero;
        ChangeState(new MoveState(this));
    }

    public void ChangeToPullingState(IPullableObject controller, Vector3 finalDestination, float remaining)
    {
        ChangeState(new BeingPulledState(controller, finalDestination, remaining, moveSpeed, cellSize));
    }
    public static bool IsPathBlocked(Transform t, float checkDistance)
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Walls");


        Vector3 rayOrigin = t.position + (Vector3.up * 0.5f);

        if (Physics.Raycast(rayOrigin, t.forward, out hit, checkDistance, layerMask))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
}