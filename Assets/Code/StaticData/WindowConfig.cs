using System;
using Code.Window;
using UnityEngine.AddressableAssets;

namespace Code.StaticData
{
    [Serializable]
    public class WindowConfig
    {
        public WindowTypeId WindowTypeId;
        public AssetReferenceGameObject PrefabReference;
    }
}