using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RobotControlsUI : MonoBehaviour, IGameSystem
{
    [SerializeField] private CanvasGroup playerControlCanvas;
    [SerializeField] private Button robotMoveControl;
    [SerializeField] private Robot robot;

    private TextMeshProUGUI _buttonLabel;

    private bool robotMoveActiveButton = true;

    private IGameEvents _gameEvents;

    private void Start()
    {
        _buttonLabel = robotMoveControl.GetComponentInChildren<TextMeshProUGUI>();
        robotMoveControl.onClick.AddListener(OnRobotMoveControlClicked);
    }

    public void Initialize(IGameEvents gameEvents)
    {
        _gameEvents = gameEvents;
        gameEvents.OnExploreEntered += OnExplore;
        gameEvents.OnPlanEntered += OnPlan;
        gameEvents.OnExecuteEntered += OnExecute;
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
        SetCanvasVisible(false);
    }

    private void OnPlan()
    {
        SetCanvasVisible(false);
    }

    private void OnExecute()
    {
        SetCanvasVisible(true);
        robotMoveActiveButton = true;
        _buttonLabel.text = "Stop";
    }

    private void OnRobotMoveControlClicked()
    {
        if (robotMoveActiveButton)
        {
            if (!robot.TryStop()) return;
            robotMoveActiveButton = false;
            _buttonLabel.text = "Move";
        }

        else
        {
            if (!robot.TryMove()) return;
            robotMoveActiveButton = true;
            _buttonLabel.text = "Idle";
        }
    }

    private void SetCanvasVisible(bool visible)
    {
        playerControlCanvas.alpha = visible ? 1f : 0f;
        playerControlCanvas.interactable = visible;
        playerControlCanvas.blocksRaycasts = visible;
    }


}
