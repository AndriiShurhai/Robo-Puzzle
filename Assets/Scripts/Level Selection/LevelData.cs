using UnityEngine;
using System.Collections.Generic; // Make sure this is here

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string LevelID;
    public string DisplayName;
    public string SceneName;

    [Header("Progression")]
    public bool UnlockedByDefault;

    [Header("Level Inventory")]
    public List<ToolLoadout> AvailableTools = new List<ToolLoadout>();
}