using UnityEngine;
using UnityEngine.UI;

public class UILevelSelection : MonoBehaviour
{
    [SerializeField] private GameObject levelNodePrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private GameObject worldSelectionScreen;
    [SerializeField] private TMPro.TextMeshProUGUI worldTitleText;

    public void ShowWorldLevels(WorldData world)
    {
        gameObject.SetActive(true);
        worldTitleText.text = world.WorldName;

       
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        for (int i = 0; i < world.LevelsInWorld.Count; i++)
        {
            LevelData level = world.LevelsInWorld[i];
            GameObject nodeObj = Instantiate(levelNodePrefab, gridContainer);

            nodeObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();

            Button btn = nodeObj.GetComponent<Button>();
            bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(level.LevelID);

            if (isUnlocked)
            {
                btn.interactable = true;
                btn.onClick.AddListener(() => LevelManager.Instance.LoadLevel(level));
            }
            else
            {
                btn.interactable = false;
            }
        }
    }

    public void BackToWorlds()
    {
        gameObject.SetActive(false);
        worldSelectionScreen.SetActive(true);
    }
}