using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;
using Newtonsoft.Json;
using UnityEngine;

namespace Code.Services.SaveLoad
{
    public class PrefsSaveLoadService : ISaveLoadService
    {
        private const string PlayerDataKey = "PlayerData";

        private readonly IPersistenceProgressService _progressService;

        public PrefsSaveLoadService(IPersistenceProgressService progressService)
        {
            _progressService = progressService;
        }

        public void SaveProgress() => Save(_progressService.PlayerData);

        public void Save(PlayerData playerData)
        {
            string json = JsonConvert.SerializeObject(playerData);
            PlayerPrefs.SetString(PlayerDataKey, json);
            PlayerPrefs.Save();
            Debug.Log($"PlayerData saved to PlayerPrefs: {PlayerDataKey}");
        }

        public PlayerData Load()
        {
            string json = PlayerPrefs.GetString(PlayerDataKey, string.Empty);
            return string.IsNullOrEmpty(json)
                ? null
                : JsonConvert.DeserializeObject<PlayerData>(json);
        }
    }
}
