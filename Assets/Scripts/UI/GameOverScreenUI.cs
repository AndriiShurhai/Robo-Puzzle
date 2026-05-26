using UnityEngine;
using UnityEngine.UI;

public class GameOverScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverScreenCanvas;
    [SerializeField] private GameObject _levelCompletePanel;
    [SerializeField] private GameObject _levelFailedPanel;

    [SerializeField] private Button _restartLevelButton;
    [SerializeField] private Button _continueButton;




    private void Start()
    {
        HideGameOverScreen();   
        GameManager.Instance.OnLevelComplete += ShowLevelComplete;
        GameManager.Instance.OnLevelFailed += ShowLevelFailed;
        _restartLevelButton.onClick.AddListener(RestartLevel);
        _continueButton.onClick.AddListener(ContinueToNextLevel);
    }

    private void ShowLevelComplete()
    {
        _gameOverScreenCanvas.SetActive(true);
        _levelCompletePanel.SetActive(true);
        _levelFailedPanel.SetActive(false);
    }

    private void ShowLevelFailed()
    {
        _gameOverScreenCanvas.SetActive(true);
        _levelCompletePanel.SetActive(false);
        _levelFailedPanel.SetActive(true);
    }

    private void RestartLevel()
    {
        HideGameOverScreen();
    }

    private void ContinueToNextLevel()
    {
        HideGameOverScreen();
    }

    private void HideGameOverScreen()
    {
        _gameOverScreenCanvas.SetActive(false);
        _levelCompletePanel.SetActive(false);
        _levelFailedPanel.SetActive(false);
    }
}
