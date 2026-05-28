using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldSelection : MonoBehaviour
{
    [SerializeField] private List<WorldData> allWorlds;
    [SerializeField] private GameObject worldButtonPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private UILevelSelection levelSelectionScreen;

    void Start()
    {
        PopulateWorlds();
    }

    private void PopulateWorlds()
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (WorldData world in allWorlds)
        {
            GameObject btnObj = Instantiate(worldButtonPrefab, container);

            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = world.WorldName;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OpenWorld(world));
        }
    }

    private void OpenWorld(WorldData world)
    {
        levelSelectionScreen.ShowWorldLevels(world);
        gameObject.SetActive(false);
    }
}