using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New World", menuName = "Game/World Data")]
public class WorldData : ScriptableObject
{
    public string WorldID;
    public string WorldName;
    public Sprite WorldBackground; 
    public List<LevelData> LevelsInWorld = new List<LevelData>();
}