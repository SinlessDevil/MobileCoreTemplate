using System;
using System.Threading;
using Code.Services.AssetPreloader;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;


namespace Code.Infrastructure
{
    public class SceneLoader : ISceneLoader
    {
        private const float MinDisplayedProgress = 0.2f;

        private readonly IAssetPreloaderService _assetPreloader;

        public SceneLoader(IAssetPreloaderService assetPreloader)
        {
            _assetPreloader = assetPreloader;
        }

        public async UniTask LoadForce(string name, Action onLevelLoad, ILoadingCurtain loadingCurtain)
        {
            await LoadLevelAsync(name, onLevelLoad, loadingCurtain, loadForce: true, isAddressable: true);
        }

        public async UniTask Load(string name, Action onLevelLoad, bool isAddressable, ILoadingCurtain loadingCurtain)
        {
            await LoadLevelAsync(name, onLevelLoad, loadingCurtain, loadForce: false, isAddressable);
        }

        private async UniTask LoadLevelAsync(string name, Action onLevelLoad, ILoadingCurtain loadingCurtain,
            bool loadForce, bool isAddressable)
        {
            if (!loadForce && SceneManager.GetActiveScene().name == name)
            {
                onLevelLoad?.Invoke();
                return;
            }

            if (isAddressable && await _assetPreloader.NeedLoadAssetsFor(name))
            {
                var cts = new CancellationTokenSource();
                FakeLoadingBarAsync(loadingCurtain, cts.Token);
                AsyncOperationStatus status = await _assetPreloader.LoadAssetsFor(name);
                cts.Cancel();

                if (status == AsyncOperationStatus.Failed)
                {
                    loadingCurtain?.ShowNoInternetWarning(() => LoadForce(name, onLevelLoad, loadingCurtain).Forget());
                    return;
                }
            }

            await LoadSceneAsync(name, onLevelLoad, loadingCurtain, isAddressable);
        }

        private async UniTask LoadSceneAsync(string name, Action onLevelLoad, ILoadingCurtain loadingCurtain,
            bool isAddressable)
        {
            if (isAddressable)
            {
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(name);
                while (!handle.IsDone)
                {
                    loadingCurtain?.ShowProgress(Mathf.Max(MinDisplayedProgress, handle.PercentComplete));
                    await UniTask.Yield();
                }

                if (!handle.Result.Scene.IsValid())
                {
                    loadingCurtain?.ShowNoInternetWarning(() => LoadForce(name, onLevelLoad, loadingCurtain).Forget());
                    return;
                }
            }
            else
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(name);
                while (!op.isDone)
                {
                    loadingCurtain?.ShowProgress(Mathf.Max(MinDisplayedProgress, op.progress));
                    await UniTask.Yield();
                }
            }

            loadingCurtain?.ShowProgress(1f);
            onLevelLoad?.Invoke();
        }

        private async UniTaskVoid FakeLoadingBarAsync(ILoadingCurtain loadingCurtain, CancellationToken ct)
        {
            float progress = MinDisplayedProgress;
            try
            {
                while (true)
                {
                    loadingCurtain?.ShowProgress(progress);
                    progress += progress < 0.5f ? Time.deltaTime * 0.02f : Time.deltaTime * 0.01f;
                    progress = Mathf.Clamp01(progress);
                    if (progress > 0.95f) progress = 0.46f;
                    await UniTask.Yield(ct);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
