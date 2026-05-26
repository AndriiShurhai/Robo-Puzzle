using UnityEngine;

public class ToolSlotsContainerUI : MonoBehaviour, IGameSystem
{
    [SerializeField] private GameObject _toolSlotsContainer;
    private IGameEvents _gameEvents;
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
