using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string LevelID;
    public string DisplayName;
    public string SceneName;
    public bool UnlockedByDefault;
}