using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/PreloadConfig", fileName = "PreloadConfig", order = 0)]
    public class PreloadConfig : ScriptableObject
    {
        public List<PreloadGroup> LevelGroups = new();
    }

    [Serializable]
    public class PreloadGroup
    {
        public string AssetGroupName;
        public int LoadAfterUnlocked;
    }
}
