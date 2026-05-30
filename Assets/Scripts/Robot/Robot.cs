using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;



public class IdleState : IState
{
    private Robot _controller;

    public IdleState(Robot controller)
    {
        this._controller = controller;
    }

    public void Enter()
    {
        Vector3 pos = _controller.transform.position;
        if (GridSnapper.IsCellCenter(new Vector3(pos.x, 0.5f, pos.z))) return;

        Vector3 snapped = GridSnapper.CellCenter(GridSnapper.WorldToCell(pos));
        Debug.Log("Snapping to " + snapped);
        _controller.transform.position = new Vector3(snapped.x, pos.y, snapped.z);
    }

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
        float step = _controller.MoveSpeed * Time.deltaTime;
        float dist = Vector3.Distance(_controller.transform.position, _targetPosition);

        if (dist <= step)
        {
            float remaining = step - dist;
            _controller.transform.position = _targetPosition;

            if (Robot.IsPathBlocked(_controller.transform, _controller.transform.forward, _controller.CellSize * 1.5f))
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

        Vector3 snappedPosition = GridSnapper.CellCenter(GridSnapper.WorldToCell(_targetPosition));
        _targetPosition = new Vector3(snappedPosition.x, _controller.transform.position.y, snappedPosition.z);
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
        Vector3 rawTarget = _controller.transform.position + (pushDirection * _controller.PushBackDistance);
        Vector3 snapped = GridSnapper.CellCenter(GridSnapper.WorldToCell(rawTarget));
        _targetPosition = new Vector3(snapped.x, _controller.transform.position.y, snapped.z);
        _timer.OnCompleted += () => _controller.ChangeState(new MoveState(_controller));
        _timer.Play(1f);
    }

    public void Update()
    {
        float step = _controller.PushBackSpeed * Time.deltaTime;
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

        Vector3 trueCenter = GridSnapper.CellCenter(GridSnapper.WorldToCell(currentPos));

        float distX = Mathf.Abs(_finalDestination.x - currentPos.x);
        float distZ = Mathf.Abs(_finalDestination.z - currentPos.z);

        if (distX > distZ)
        {
            _alignTarget = new Vector3(
                trueCenter.x,
                currentPos.y,
                _finalDestination.z
            );
        }
        else
        {
            _alignTarget = new Vector3(
                _finalDestination.x,
                currentPos.y,
                trueCenter.z
            );
        }
    }

    public void Update()
    {
        Transform controllerTransform = _controller.GetTransform();
        float step = _alignSpeed * Time.deltaTime;
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
        this._cellSize = Mathf.RoundToInt(cellSize);
    }

    public void Enter()
    {
        SetNextTargetCell();
        _controller.GetTransform().position = Vector3.MoveTowards(_controller.GetTransform().position, _targetPosition, _carryoverBudget);
    }

    public void Update()
    {
        Transform controllerTransform = _controller.GetTransform();
        float step = _moveSpeed * Time.deltaTime;
        float dist = Vector3.Distance(controllerTransform.position, _targetPosition);

        if (dist <= step)
        {
            float remaining = step - dist;
            controllerTransform.position = _targetPosition;

            if (Vector3.Distance(controllerTransform.position, _finalDestination) < 0.1f)
            {
                _controller.StopPulling();
                return;
            }

            Vector3 pullDir = (_finalDestination - controllerTransform.position).normalized;
            pullDir = new Vector3(Mathf.Round(pullDir.x), 0, Mathf.Round(pullDir.z));

            if (Robot.IsPathBlocked(controllerTransform, pullDir, _cellSize * 1.5f))
            {
                _controller.StopPulling();
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
        directionToDestination = new Vector3(Mathf.Round(directionToDestination.x), 0, Mathf.Round(directionToDestination.z));
        _targetPosition = _controller.GetTransform().position + (directionToDestination * _cellSize);
    }

    public void Exit() { }
}

public class Robot : MonoBehaviour, IPullableObject, IGameSystem, IDirectable
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

    private IGameEvents _gameEvents;
    private IState _currentState;
    private Vector3 _pullDestination;
    private Vector3? _spawnPosition;
    private Quaternion? _spawnRotation;
    private Coroutine _alignCoroutine;

    private static int _wallLayerMask;

    private void Awake()
    {
        _spawnPosition = null;
        _spawnRotation = null;
        ChangeState(new IdleState(this));
    }

    void Start()
    {
        _wallLayerMask = LayerMask.GetMask("Walls");
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
    }

    public void Initialize(IGameEvents gameEvents)
    {
        _gameEvents = gameEvents;

        _gameEvents.OnExploreEntered += OnExplore;
        _gameEvents.OnPlanEntered += OnPlan;
        _gameEvents.OnExecuteEntered += OnExecute;
    }

    private void OnDestroy()
    {
        _spawnPosition = null;
        _spawnRotation = null;
        if (_gameEvents == null) return;
        _gameEvents.OnExploreEntered -= OnExplore;
        _gameEvents.OnPlanEntered -= OnPlan;
        _gameEvents.OnExecuteEntered -= OnExecute;
    }

    private void OnExplore()
    {
        _alignCoroutine = null;
        if (_spawnPosition != null || _spawnRotation != null)
        {
            transform.SetPositionAndRotation(_spawnPosition.Value, _spawnRotation.Value);
        }
        ChangeState(new IdleState(this));
    }

    private void OnPlan()
    {
        _alignCoroutine = null;
        if (_spawnPosition != null || _spawnRotation != null)
        {
            transform.SetPositionAndRotation(_spawnPosition.Value, _spawnRotation.Value);
        }
        ChangeState(new IdleState(this));
    }

    private void OnExecute()
    {
        Debug.Log("Executing");
        ChangeState(new MoveState(this));
    }

    void Update()
    {
        _currentState?.Update();
    }

    public void ChangeState(IState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void SetDirection(Vector3 direction, Transform platform)
    {
        if (!(_currentState is MoveState)) return;

        Vector3 pos = platform.position;
        Vector3 snappedCell = GridSnapper.CellCenter(GridSnapper.WorldToCell(pos));
        Vector3 targetPosition = new Vector3(snappedCell.x, transform.position.y, snappedCell.z);

        Quaternion targetRotation = Quaternion.LookRotation(SnapToCardinal(direction), Vector3.up);

        ChangeState(new IdleState(this));

        if (_alignCoroutine != null)
        {
            StopCoroutine(_alignCoroutine);
        }

        _alignCoroutine = StartCoroutine(SmoothAlignRoutine(targetPosition, targetRotation));
    }

    private IEnumerator SmoothAlignRoutine(Vector3 targetPos, Quaternion targetRot)
    {
        float elapsed = 0f;
        float duration = 0.15f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPos, targetPos, easeT);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, easeT);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        _alignCoroutine = null;

        ChangeState(new MoveState(this));
    }

    private static Vector3 SnapToCardinal(Vector3 dir)
    {
        dir.y = 0f;
        dir.Normalize();
        return Mathf.Abs(dir.x) > Mathf.Abs(dir.z)
            ? new Vector3(Mathf.Sign(dir.x), 0f, 0f)
            : new Vector3(0f, 0f, Mathf.Sign(dir.z));
    }

    public void PullTowardsTarget(Vector3 target)
    {
        if (_pullDestination == target) return;
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

    public bool TryStop()
    {
        if (!(_currentState is MoveState))
        {
            Debug.Log("Current state is not MoveState, ignoring stop command");
            return false;
        }
        ChangeState(new IdleState(this));
        return true;
    }

    public bool TryMove()
    {
        if (!(_currentState is IdleState))
        {
            Debug.Log("Current state is not IdleState, ignoring move command");
            return false;
        }
        ChangeState(new MoveState(this));
        return true;
    }

    public void ChangeToPullingState(IPullableObject controller, Vector3 finalDestination, float remaining)
    {
        ChangeState(new BeingPulledState(controller, finalDestination, remaining, moveSpeed, cellSize));
    }

    public static bool IsPathBlocked(Transform t, Vector3 direction, float checkDistance)
    {
        RaycastHit hit;
        Vector3 rayOrigin = t.position + (Vector3.up * 0.5f);

        if (Physics.Raycast(rayOrigin, direction, out hit, checkDistance, _wallLayerMask))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
}