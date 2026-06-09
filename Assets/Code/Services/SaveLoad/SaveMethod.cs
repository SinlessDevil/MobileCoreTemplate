using System;

namespace Code.Services.SaveLoad
{
    [Serializable]
    public enum SaveMethod
    {
        PlayerPrefs,
        Json
    }
}