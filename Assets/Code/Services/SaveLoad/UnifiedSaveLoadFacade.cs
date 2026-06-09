using System;
using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;

namespace Code.Services.SaveLoad
{
    public class UnifiedSaveLoadFacade : ISaveLoadFacade
    {
        private readonly ISaveLoadService _prefsService;
        private readonly ISaveLoadService _jsonService;
        private readonly IPersistenceProgressService _progressService;

        public UnifiedSaveLoadFacade(
            IPersistenceProgressService progressService)
        {
            _progressService = progressService;
            _prefsService = new PrefsSaveLoadService(_progressService);
            _jsonService = new JsonSaveLoadService(_progressService);
        }

        public void SaveProgress(SaveMethod method)
        {
            Save(method, _progressService.PlayerData);
        }
        
        public void Save(SaveMethod method, PlayerData data)
        {
            GetService(method).Save(data);
        }

        public PlayerData Load(SaveMethod method)
        {
            return GetService(method).Load();
        }
        
        private ISaveLoadService GetService(SaveMethod method) => method switch
        {
            SaveMethod.PlayerPrefs => _prefsService,
            SaveMethod.Json        => _jsonService,
            _ => throw new Exception("Unknown save method.")
        };
    }
}