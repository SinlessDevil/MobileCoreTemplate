using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Code.Services.AssetPreloader
{
    public interface IAssetPreloaderService
    {
        UniTask<bool> NeedLoadAssetsFor(string address);
        UniTask<bool> NeedLoadAssetsFor(AssetReference assetReference);
        UniTask<AsyncOperationStatus> LoadAssetsFor(string address);
        UniTask<AsyncOperationStatus> LoadAssetsFor(AssetReference assetReference);
        bool IsLoading(string address);
        bool IsLoading(AssetReference assetReference);
        DownloadStatus GetLoadingProgress(string address);
        DownloadStatus GetLoadingProgress(AssetReference assetReference);
    }
}
