using System.Collections.Generic;
using UnityEngine;

public class ToolSlotsContainerUI : MonoBehaviour, IGameSystem
{
    public static ToolSlotsContainerUI Instance { get; private set; }

    [SerializeField] private GameObject _toolSlotsContainer;
    [SerializeField] private GameObject _toolSlotPrefab; // Assign your ToolSlotUI prefab here
    [SerializeField] private Transform _slotsParent; // The LayoutGroup transform

    [SerializeField] private ToolsManager _toolsManager; 

    private IGameEvents _gameEvents;
    private Dictionary<ToolDefinition, ToolSlotUI> _spawnedSlots = new Dictionary<ToolDefinition, ToolSlotUI>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (_toolsManager == null)
        {
            _toolsManager = ToolsManager.Instance;
            _toolsManager.OnInventoryChanged += HandleInventoryChanged;
        }
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

    public void BuildSlots(List<ToolLoadout> levelLoadout)
    {
        // Clear old slots
        foreach (Transform child in _slotsParent)
        {
            Destroy(child.gameObject);
        }
        _spawnedSlots.Clear();

        // Spawn new ones
        foreach (var item in levelLoadout)
        {
            GameObject slotObj = Instantiate(_toolSlotPrefab, _slotsParent);
            ToolSlotUI slotUI = slotObj.GetComponent<ToolSlotUI>();

            slotUI.Setup(item.tool, item.startingAmount);
            _spawnedSlots.Add(item.tool, slotUI);
        }
    }

    private void HandleInventoryChanged(ToolDefinition tool, int newAmount)
    {
        if (_spawnedSlots.TryGetValue(tool, out ToolSlotUI slot))
        {
            slot.UpdateAmount(newAmount);
        }
    }

    private void OnExplore()
    {
        _toolSlotsContainer.SetActive(false);
    }

    private void OnPlan()
    {
        _toolSlotsContainer.SetActive(true);
    }

    private void OnExecute()
    {
        _toolSlotsContainer.SetActive(false);
    }
}
