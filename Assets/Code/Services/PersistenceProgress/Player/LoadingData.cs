using System;
using System.Collections.Generic;

namespace Code.Services.PersistenceProgress.Player
{
    [Serializable]
    public class LoadingData
    {
        public string Version;
        public List<string> LoadedKeys = new();

        public void Reset(string currentVersion)
        {
            Version = currentVersion;
            LoadedKeys.Clear();
        }
    }
}
