using UnityEngine;


/// <summary>
/// Dual-mode camera controller. Reads all input from GameInput — owns zero
/// InputSystem calls itself.
///
/// FREE MODE  — Orbit-pivot system. Camera revolves around a ground focus point.
///   Pan      : WASD / Arrows  OR  middle-mouse drag  (via GameInput)
///   Zoom     : Scroll wheel
///   Orbit    : Right-mouse drag
///
/// ISOMETRIC FOLLOW MODE — Fixed angle, smoothly tracks a world target.

/// </summary>
/// 

public class CameraController : MonoBehaviour, IGameSystem
{
    public enum CameraMode { Free, Follow }

    [Header("Mode")]
    [SerializeField] private CameraMode startingMode = CameraMode.Free;

    [Header("Free - Pan")]
    [SerializeField] private float panSpeed = 25f;
    [SerializeField] private float panSmoothing = 10f;
    [SerializeField] private float mouseDragPanSpeed = 0.4f;
    [SerializeField] private bool enableEdgeScroll = false;
    [SerializeField] private float edgeThreshold = 15f; //px from screen edge to trigger edge scroll

    [Header("Free - Zoom")]
    [SerializeField] private float zoomSpeed = 600f;
    [SerializeField] private float minZoomDistance = 5f;
    [SerializeField] private float maxZoomDistance = 60f;
    [SerializeField] private float zoomSmoothing = 10f;

    [Header("Free - Orbit (right-mouse drag)")]
    [SerializeField] private float orbitSensitivity = 0.2f;
    [SerializeField] private float minPitch = 15f;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float rotSmoothing = 12f;

    [Header("Isometric Follow")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 followOffset = new Vector3 (-10f, 20f, -10f);
    [SerializeField] private float followSmoothing = 6f;
    [SerializeField] private Vector3 isoEuler = new Vector3(45f, 45f, 0f);

    private CameraMode _currentMode;

    private Vector3 _focusPoint;
    private float _targetYaw;
    private float _targetPitch;
    private float _targetDistance;

    private Vector3 _smoothFocus;
    private float _smoothYaw;
    private float _smoothPitch;
    private float _smoothDistance;

    private Camera _cam;

    private IGameEvents _gameEvents;

    private void Awake()
    {
        _cam = Camera.main;
        SyncInternalStateFromTransform();
        _currentMode = startingMode;
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
        if (_gameEvents == null) return;
        _gameEvents.OnExploreEntered -= OnExplore;
        _gameEvents.OnPlanEntered -= OnPlan;
        _gameEvents.OnExecuteEntered -= OnExecute;
    }
    private void Update()
    {
        if (_currentMode == CameraMode.Free)
        {
            HandlePan();
            HandleZoom();
            HandleOrbit();
            ApplyFreeCamera();   
        }

        else
        {
            ApplyIsometricFollow();
        }
    }

    private void OnExplore()
    {
        SetMode(CameraMode.Free);
    }

    private void OnPlan()
    {
        SetMode(CameraMode.Free);
    }

    private void OnExecute()
    {
        SetMode(CameraMode.Follow);
    }

    private void HandlePan()
    {
        if (GameInput.Instance == null) return;

        Vector3 input = Vector3.zero;

        // Keyboard / gamepad
        Vector2 kb = GameInput.Instance.CameraPan;

        input.x += kb.x;
        input.z += kb.y;


        // Middle-mouse drag
        if (GameInput.Instance.CameraDragHeld)
        {
            Vector2 delta = GameInput.Instance.MouseDelta;
            input.x -= delta.x * mouseDragPanSpeed;
            input.z -= delta.y * mouseDragPanSpeed;
        }

        // Edge scroll
        if (enableEdgeScroll && Cursor.lockState == CursorLockMode.None)
        {
            Vector2 mp = GameInput.Instance.MousePosition;

            if (mp.x < edgeThreshold) input.x -= 1f;
            if (mp.x > Screen.width - edgeThreshold) input.x += 1f;
            if (mp.y < edgeThreshold) input.z -= 1f;
            if (mp.y > Screen.height - edgeThreshold) input.z += 1f; 
        }

        if (input.sqrMagnitude < 0.0001f) return;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        _focusPoint += (right * input.x + forward * input.z) * panSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        if (GameInput.Instance == null) return;

        float scroll = GameInput.Instance.CameraZoom;
        if (Mathf.Abs(scroll) < 0.0001f) return;

        _targetDistance -= scroll * zoomSpeed * Time.deltaTime;
        _targetDistance = Mathf.Clamp(_targetDistance, minZoomDistance, maxZoomDistance);
    }

    private void HandleOrbit()
    {
        if (GameInput.Instance == null) return;
        if (!GameInput.Instance.CameraOrbitHeld) return;

        Vector2 delta = GameInput.Instance.MouseDelta;
        _targetYaw += delta.x * orbitSensitivity;
        _targetPitch -= delta.y * orbitSensitivity;
        _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);
    }

    private void ApplyFreeCamera()
    {
        float dt = Time.deltaTime;

        _smoothFocus = Vector3.Lerp(_smoothFocus, _focusPoint, panSmoothing * dt);
        _smoothYaw = Mathf.LerpAngle(_smoothYaw, _targetYaw, rotSmoothing * dt);
        _smoothPitch = Mathf.Lerp(_smoothPitch, _targetPitch, rotSmoothing * dt);
        _smoothDistance = Mathf.Lerp(_smoothDistance, _targetDistance, zoomSmoothing * dt);

        Quaternion rot = Quaternion.Euler(_smoothPitch, _smoothYaw, 0f);
        Vector3 pos = _smoothFocus - rot * Vector3.forward * _smoothDistance;

        if (_cam.orthographic)
        {
            _cam.orthographicSize = _smoothDistance;
        }

        transform.SetPositionAndRotation(pos, rot);
    }

    private void ApplyIsometricFollow()
    {
        if (followTarget == null) return;

        float dt = Time.deltaTime;

        Vector3 targetPosition = followTarget.position + followOffset;
        Quaternion targetRot = Quaternion.Euler(isoEuler);

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothing * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, followSmoothing * dt);
    }

    public void SetMode(CameraMode mode, Transform target = null)
    {
        if (target != null) followTarget = target;
        _currentMode = mode;
        if (mode == CameraMode.Free)
        {
            SyncInternalStateFromTransform();
        }
    }

    public void TeleportFocusPoint(Vector3 worldPosition)
    {
        _focusPoint = worldPosition;
        _smoothFocus = worldPosition;
    }
    public Vector3 GetFocusPoint() => _smoothFocus;

    private void SyncInternalStateFromTransform()
    {
        _targetYaw = transform.eulerAngles.y;
        _targetPitch = transform.eulerAngles.x;
        _focusPoint = EstimateGroundFocus();
        _targetDistance = Vector3.Distance(transform.position, _focusPoint);

        _smoothYaw = _targetYaw;
        _smoothPitch = _targetPitch;
        _smoothFocus = _focusPoint;
        _smoothDistance = _targetDistance;
    }



    private Vector3 EstimateGroundFocus()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 300f)) return hit.point;

        if (Mathf.Abs(transform.forward.y) > 0.0001f)
        {
            float t = -transform.position.y / transform.forward.y;
            if (t > 0f) return transform.position + transform.forward * t;
        }

        return transform.position + transform.forward * 20f;
    }
}