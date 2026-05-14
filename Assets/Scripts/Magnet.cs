using UnityEngine;
using System.Collections.Generic;
public interface ISwitchableToolState
{
    void Enter();
    void Update();
    void Exit();
}

public interface IPullableObject
{
    Transform GetTransform();
    Vector3? GetDestination();
    void PullTowardsTarget(Vector3 target);
    void ChangeToPullingState(IPullableObject controller, Vector3 finalDestination, float remaining);
    void StopPulling();
}

public interface ISwitchableTool
{
    void ToggleSwitch();
}

public struct PullData
{
    public IPullableObject Pullable;
    public float Distance;
}

public class MagnetNotActiveState : ISwitchableToolState
{
    private Magnet controller;

    public MagnetNotActiveState(Magnet controller)
    {
        this.controller = controller;
    }

    public void Enter() 
    {
        Debug.Log("Magnet not active state update");
        Vector3 origin = controller.transform.position + (Vector3.up * 0.5f);
        int layerMask = LayerMask.GetMask("Magnetic Objects");

        Debug.DrawRay(origin, controller.transform.forward * controller.PullRange, Color.red);
        RaycastHit[] hits = Physics.BoxCastAll(origin, controller.BeamHalfExtents, controller.transform.forward, controller.transform.rotation, controller.PullRange, layerMask);
        if (hits.Length > 0)
        {
            List<PullData> pullableObjects = new List<PullData>();

            foreach (var hit in hits)
            {
                IPullableObject obj = hit.collider.GetComponent<IPullableObject>();
                if (obj != null)
                {
                    float dist = Vector3.Distance(controller.transform.position, hit.transform.position);
                    pullableObjects.Add(new PullData { Pullable = obj, Distance = dist });
                }

            }

            pullableObjects.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            for (int i = 0; i < pullableObjects.Count; i++)
            {
                pullableObjects[i].Pullable.StopPulling();
            }
        }
        else
        {
        }
    }

    public void Update()
    {
    }

    public void Exit() { }
}

public class MagnetActiveState : ISwitchableToolState
{
    private Magnet controller;
    private IPullableObject currentlyPulledObject;
    private List<IPullableObject> currentlyPulledObjects;
    public MagnetActiveState(Magnet controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Enter active magnet state");
        currentlyPulledObjects = new List<IPullableObject>();
    }

    public void Update()
    {
        Debug.Log("Magnet active state update");
        Vector3 origin = controller.transform.position + (Vector3.up * 0.5f);
        int layerMask = LayerMask.GetMask("Magnetic Objects");

        Debug.DrawRay(origin, controller.transform.forward * controller.PullRange, Color.red);
        RaycastHit[] hits = Physics.BoxCastAll(origin, controller.BeamHalfExtents, controller.transform.forward, controller.transform.rotation, controller.PullRange, layerMask);
        if (hits.Length > 0)
        {
            List<PullData> pullableObjects = new List<PullData>();

            foreach (var hit in hits)
            {
                IPullableObject obj = hit.collider.GetComponent<IPullableObject>();
                if (obj != null && !currentlyPulledObjects.Contains(obj))
                {
                    float dist = Vector3.Distance(controller.transform.position, hit.transform.position);
                    pullableObjects.Add(new PullData { Pullable = obj, Distance = dist });
                    currentlyPulledObjects.Add(obj);
                }

            }

            pullableObjects.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            for (int i = 0; i < pullableObjects.Count; i++)
            {
                float queueOffset = controller.HoldingDistance + (i * 1f);
                Vector3 destination = controller.transform.position + (controller.transform.forward * queueOffset);

                Debug.Log("Destination for " + pullableObjects[i].Pullable.GetTransform().name + ": " + destination);
                pullableObjects[i].Pullable.PullTowardsTarget(destination);
            }
        }
        else
        {
        }
    }

    public void Exit()
    {
        currentlyPulledObjects.Clear();
    }
}

public class Magnet : MonoBehaviour, ISwitchableTool
{
    [Header("Magnet Settings")]
    public float HoldingDistance = 1f;
    public float PullRange = 5f;
    public Vector3 BeamHalfExtents = new Vector3(0.5f, 0.5f, 0.1f);

    private ISwitchableToolState currentState;

    private void Start()
    {
        ChangeState(new MagnetNotActiveState(this));
    }

    private void FixedUpdate()
    {
        currentState?.Update();
    }

    private void ChangeState(ISwitchableToolState newState)
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