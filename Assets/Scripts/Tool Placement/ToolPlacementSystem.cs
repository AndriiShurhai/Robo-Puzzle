using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class ToolPlacementSystem : MonoBehaviour
{
    public static ToolPlacementSystem Instance { get; private set; }

    public event Action<ToolDefinition, GameObject> OnToolPlaced;

    [Header("References")]
    [SerializeField] private GhostController ghost;
    [SerializeField] private Camera gameCamera;
        
    [Header("Raycasting")]
    [Tooltip("Only surfaces on these layers are valid placement targets")]
    [SerializeField] private LayerMask placeableLayers;
    [SerializeField] private float maxRayDistance = 100f;

    private ToolDefinition _activeTool;
    private PlacementContext _lastContext;
    private bool _isPlacing;
    private bool _lastWasValid;

    private readonly HashSet<Vector3Int> _occupiedCells = new HashSet<Vector3Int>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ghost.Hide();
    }

    private void Update()
    {
        if (!_isPlacing) return;
        UpdateGhost();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    
    public void BeginPlacement(ToolDefinition tool)
    {
        if (tool == null) return;

        _activeTool = tool;
        _isPlacing = true;
        _lastWasValid = false;
        ghost.Show(tool);
    }
    public void EndPlacement()
    {
        if (_isPlacing && _lastWasValid)
        {
            CommitPlacement();
        }
        StopPlacement();
    }
    public void CancelPlacement()
    {
        StopPlacement();
    }
    public bool IsCellOccuped(Vector3Int cell)
    {
        return _occupiedCells.Contains(cell);
    }

    public void FreeCells(IEnumerable<Vector3Int> cells)
    {
        foreach (Vector3Int c in cells) _occupiedCells.Remove(c);
    }
    private void UpdateGhost()
    {
        Ray ray = gameCamera.ScreenPointToRay(GameInput.Instance.MousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, placeableLayers))
        {
            ghost.SetValid(false);
            _lastWasValid = false;
            return;
        }

        Vector3Int cell = GridSnapper.WorldToCell(hit.point, hit.normal);
        Vector3 pos = GridSnapper.CellCenter(cell);
        Quaternion rot = GridSnapper.SurfaceRotation(hit.normal);
        SurfaceType surface = GridSnapper.ClassifySurface(hit.normal);

        _lastContext = new PlacementContext(cell, pos, rot, hit.normal, surface, hit);

        bool valid = _activeTool.IsValidPlacement(_lastContext);
        _lastWasValid = valid;

        ghost.SetPose(pos, rot);
        ghost.SetValid(valid);
    }
    private void CommitPlacement()
    {
        GameObject placed = Instantiate(
            _activeTool.toolPrefab,
            _lastContext.SnappedPosition,
            _lastContext.SnappedRotation);

        _occupiedCells.Add(_lastContext.Cell);

        OnToolPlaced?.Invoke(_activeTool, placed);
        Debug.Log($"[ToolPlacement] Placed {_activeTool.displayName} at cell {_lastContext.Cell}");
    }
    private void StopPlacement()
    {
        _isPlacing = false;
        _activeTool = null;
        ghost.Hide();
    }
}