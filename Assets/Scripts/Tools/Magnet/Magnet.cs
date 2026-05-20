using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;


public class MagnetNotActiveState : IState
{
    private Magnet controller;

    public MagnetNotActiveState(Magnet controller)
    {
        this.controller = controller;
    }

    public void Enter() 
    {
        Vector3 origin = controller.transform.position + (Vector3.up * 0.5f);
        int layerMask = LayerMask.GetMask("Magnetic Objects");

        Debug.DrawRay(origin, controller.transform.forward * controller.PullRange, Color.red);
        RaycastHit[] hits = Physics.BoxCastAll(origin, controller.BeamHalfExtents, controller.transform.forward, controller.transform.rotation, controller.PullRange, layerMask);

        foreach (RaycastHit hit in hits)
        {
            IPullableObject obj = hit.collider.GetComponent<IPullableObject>();
            obj?.StopPulling();
        }
    }

    public void Update()
    {
    }

    public void Exit() { }
}

public class MagnetActiveState : IState
{
    private Magnet _controller;

    private List<IPullableObject> _pulledObjects = new();
    private List<PullData> _newlyDetected = new();
    public MagnetActiveState(Magnet controller)
    {
        this._controller = controller;
    }

    public void Enter()
    {
        _pulledObjects.Clear();
    }

    public void Update()
    {
        Vector3 origin = _controller.transform.position + (Vector3.up * 0.5f);
        int layerMask = LayerMask.GetMask("Magnetic Objects");

        Debug.DrawRay(origin, _controller.transform.forward * _controller.PullRange, Color.red);
        RaycastHit[] hits = Physics.BoxCastAll(origin, _controller.BeamHalfExtents, _controller.transform.forward, _controller.transform.rotation, _controller.PullRange, layerMask);
        
        _newlyDetected.Clear();
        foreach (RaycastHit hit in hits)
        {
            IPullableObject obj = hit.collider.GetComponent<IPullableObject>();
            if (obj == null || _pulledObjects.Contains(obj)) continue;

            float dist = Vector3.Distance(_controller.transform.position, hit.transform.position);
            _newlyDetected.Add(new PullData { Pullable = obj, Distance = dist });
            _pulledObjects.Add(obj);
        }

        _newlyDetected.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        Vector3Int basePosition = Vector3Int.RoundToInt(_controller.transform.position);
        for (int i = 0; i < _newlyDetected.Count; i++)
        {
            float queueOffset = _controller.HoldingDistance + (i * 1f);
            Vector3 destination = basePosition + (_controller.transform.forward * queueOffset);

            _newlyDetected[i].Pullable.PullTowardsTarget(destination);
        }
    }

    public void Exit()
    {
        _pulledObjects.Clear();
    }
}

public class Magnet : MonoBehaviour, ISwitchableTool
{
    [Header("Magnet Settings")]
    [SerializeField] private float holdingDistance = 1f;
    [SerializeField] private float pullRange = 5f;
    [SerializeField] private Vector3 beamHalfExtents = new Vector3(0.5f, 0.5f, 0.1f);

    public float HoldingDistance => holdingDistance;
    public float PullRange => pullRange;
    public Vector3 BeamHalfExtents => beamHalfExtents;

    private IState currentState;

    private void Start()
    {
        ChangeState(new MagnetNotActiveState(this));
    }

    private void FixedUpdate()
    {
        currentState?.Update();
    }

    private void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void ToggleSwitch()
    {
        if (currentState is MagnetNotActiveState)
        {
            ChangeState(new MagnetActiveState(this));
        }
        else
        {
            ChangeState(new MagnetNotActiveState(this));
        }
    }

    void OnDrawGizmos()
    {
        Vector3 origin = transform.position + (Vector3.up * 0.5f);

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(origin, transform.rotation, Vector3.one);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, BeamHalfExtents * 2f);
        Gizmos.DrawWireCube(Vector3.forward * PullRange, BeamHalfExtents * 2f);

        Gizmos.matrix = oldMatrix;
    }
}