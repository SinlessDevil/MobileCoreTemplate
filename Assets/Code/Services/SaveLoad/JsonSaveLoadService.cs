using System;
using System.IO;
using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;
using Newtonsoft.Json;
using UnityEngine;

namespace Code.Services.SaveLoad
{
    public class JsonSaveLoadService : ISaveLoadService
    {
        private const string FileName = "player_data.json";

        private readonly IPersistenceProgressService _progressService;
        private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public JsonSaveLoadService(IPersistenceProgressService progressService)
        {
            _progressService = progressService;
        }

        public void SaveProgress() => Save(_progressService.PlayerData);

        public void Save(PlayerData playerData)
        {
            try
            {
                string json = JsonConvert.SerializeObject(playerData, Formatting.Indented);
                File.WriteAllText(FilePath, json);
                Debug.Log($"JSON Save complete at: {FilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save JSON: {e.Message}");
            }
        }

        public PlayerData Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Debug.LogWarning($"JSON file not found at: {FilePath}");
                    return null;
                }

                string json = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<PlayerData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load JSON: {e.Message}");
                return null;
            }
        }
    }
}
