using System.Collections.Generic;
using System.Linq;
using Code.Services.AssetPreloader;
using Code.Services.PersistenceProgress;
using Code.Services.SaveLoad;
using Code.Services.StaticData;
using Code.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Code.Services.PreloaderConductor
{
    public class AssetPreloaderConductor : IAssetPreloaderConductor
    {
        private readonly IAssetPreloaderService _assetPreloaderService;
        private readonly IStaticDataService _staticDataService;
        private readonly IPersistenceProgressService _progressService;
        private readonly ISaveLoadFacade _saveLoadFacade;

        public AssetPreloaderConductor(
            IAssetPreloaderService assetPreloaderService,
            IStaticDataService staticDataService,
            IPersistenceProgressService progressService,
            ISaveLoadFacade saveLoadFacade)
        {
            _assetPreloaderService = assetPreloaderService;
            _staticDataService = staticDataService;
            _progressService = progressService;
            _saveLoadFacade = saveLoadFacade;
        }

        public void TryPreload()
        {
            TryPreloadByLevel();
        }

        private void TryPreloadByLevel()
        {
            int currentLevel = _progressService.PlayerData.PlayerLevelData.CurrentProgress.LevelId;
            PreloadLevelDependency(currentLevel);
        }

        private async void PreloadLevelDependency(int level)
        {
            foreach (PreloadGroup group in LevelConfigsForPreload(level))
                await PreloadDependency(group);
        }

        private async UniTask PreloadDependency(PreloadGroup config)
        {
            if (await _assetPreloaderService.NeedLoadAssetsFor(config.AssetGroupName))
            {
                Debug.Log($"Start Preload: {config.AssetGroupName}");
                AsyncOperationStatus status = await _assetPreloaderService.LoadAssetsFor(config.AssetGroupName);

                if (status == AsyncOperationStatus.Succeeded)
                    RegisterAsPreloaded(config);
            }
            else
            {
                RegisterAsPreloaded(config);
            }
        }

        private void RegisterAsPreloaded(PreloadGroup config)
        {
            _progressService.PlayerData.Loading.Version = Application.version;
            _progressService.PlayerData.Loading.LoadedKeys.Add(config.AssetGroupName);
            _saveLoadFacade.SaveProgress(SaveMethod.PlayerPrefs);
        }

        private IEnumerable<PreloadGroup> LevelConfigsForPreload(int level) =>
            _staticDataService.PreloadConfig.LevelGroups.Where(x =>
            {
                bool isAlreadyPreloaded = _progressService.PlayerData.Loading.LoadedKeys.Contains(x.AssetGroupName);
                return x.LoadAfterUnlocked <= level && !isAlreadyPreloaded;
            });
    }
}
