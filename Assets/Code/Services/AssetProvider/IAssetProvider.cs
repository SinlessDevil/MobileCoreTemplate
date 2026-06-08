using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Code.Services.AssetProvider
{
    public interface IAssetProvider
    {
        void Initialize();
        UniTask<T> Load<T>(AssetReference assetReference) where T : class;
        UniTask<T> Load<T>(string address) where T : class;
        void CleanUp();
        UniTask<T> LoadPersistence<T>(AssetReference assetReference) where T : class;
        UniTask<T> LoadPersistence<T>(string address) where T : class;
        void MakeTemp(AssetReference assetReference);
        void MakePersistence(AssetReference assetReference);
        string Progress { get; }
    }
}
