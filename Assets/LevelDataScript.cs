using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public int birdOrder = -1;
    public int slotIndex = -1;
}

[Serializable]
public class LevelIconData
{
    public int placeholderIndex;
    public string iconName;
    public string levelID;
}

[Serializable]
public class LevelData
{
    public List<LevelObjectData> objects = new List<LevelObjectData>();
    public List<LevelIconData> icons = new List<LevelIconData>();

    public string levelName;
}