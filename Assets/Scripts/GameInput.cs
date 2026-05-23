using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ── Camera map ───────────────────────────────────────────────────────────────
///   Pan          Vector2   WASD + Arrow keys (Up/Down/Left/Right composite)
///   Zoom         float     <Mouse>/scroll/y
///   OrbitHold    Button    <Mouse>/rightButton
///   DragHold     Button    <Mouse>/middleButton
///   MouseDelta   Vector2   <Mouse>/delta          (Passthrough, no processor)
///   MousePosition Vector2  <Mouse>/position       (Passthrough)
/// </summary>

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap _cameraMap;
    private InputActionMap _toolsMap;

    private InputAction _pan;
    private InputAction _zoom;
    private InputAction _orbitHeld;
    private InputAction _dragHeld;
    private InputAction _mouseDelta;
    private InputAction _mousePosition;


    private InputAction _rotateButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inputActions == null)
        {
            Debug.LogError("[GameInput] No InputActionAsset assigned in inspector.", this);
            return;
        }

        BindActions();
    }

    private void Start()
    {
    }

    private void OnEnable()
    {
        _cameraMap?.Enable();
        _toolsMap?.Enable();
    }

    private void OnDisable()
    {
        _cameraMap?.Disable();
        _toolsMap?.Disable();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BindActions()
    {
        _cameraMap = inputActions.FindActionMap("Camera", throwIfNotFound: true);
        _toolsMap = inputActions.FindActionMap("Tools", throwIfNotFound: true);

        _pan = _cameraMap.FindAction("Pan", throwIfNotFound: true);
        _zoom = _cameraMap.FindAction("Zoom", throwIfNotFound: true);
        _orbitHeld = _cameraMap.FindAction("OrbitHold", throwIfNotFound: true);
        _dragHeld = _cameraMap.FindAction("DragHold", throwIfNotFound: true);
        _mouseDelta = _cameraMap.FindAction("MouseDelta", throwIfNotFound: true);
        _mousePosition = _cameraMap.FindAction("MousePosition", throwIfNotFound: true);

        _rotateButton = _toolsMap.FindAction("Rotate", throwIfNotFound: true);
    }

    // WASD / Arrow key pan input. Range [-1, 1] on each axis.
    public Vector2 CameraPan => _pan.ReadValue<Vector2>();

    // Scroll wheel zoom delta this frame. Positive = zoom in, Negative = zoom out.
    public float CameraZoom => _zoom.ReadValue<float>();

    // True while the right mouse button is held (orbit mode).
    public bool CameraOrbitHeld => _orbitHeld.IsPressed();
    
    // True while the middle mouse button is held (drag-pan mode).
    public bool CameraDragHeld => _dragHeld.IsPressed();

    // Raw mouse movement delta this frame.
    public Vector2 MouseDelta => _mouseDelta.ReadValue<Vector2>();

    // Mouse cursor position in screen space (pixels, origin bottom-left). Note that this is not necessarily the same as the mouse position used by the Camera's ScreenToWorldPoint() if the camera is moving or rotating while the mouse is stationary.
    public Vector2 MousePosition => _mousePosition.ReadValue<Vector2>();
    
    public bool RotateButtonPressed => _rotateButton.triggered;

    public void SetCameraInputActive(bool active)
    {
        if (active) _cameraMap.Enable();
        else _cameraMap.Disable();
    }


}