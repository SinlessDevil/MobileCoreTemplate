using Code.Services.AssetProvider;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Zenject;

namespace Code.Services.Factories
{
    public abstract class Factory
    {
        private readonly IInstantiator _instantiator;
        private readonly IAssetProvider _assetProvider;

        protected Factory(IInstantiator instantiator, IAssetProvider assetProvider)
        {
            _instantiator = instantiator;
            _assetProvider = assetProvider;
        }

        protected async UniTask<GameObject> Instantiate(string address)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            return MoveToCurrentScene(_instantiator.InstantiatePrefab(prefab));
        }

        protected async UniTask<GameObject> Instantiate(string address, Transform parent)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            return MoveToCurrentScene(_instantiator.InstantiatePrefab(prefab, parent));
        }

        protected async UniTask<GameObject> Instantiate(string address, Transform parent, bool isCanvas)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            GameObject instance = _instantiator.InstantiatePrefab(prefab, parent);
            return isCanvas ? instance : MoveToCurrentScene(instance);
        }

        protected async UniTask<GameObject> Instantiate(string address, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            return MoveToCurrentScene(_instantiator.InstantiatePrefab(prefab, position, rotation, parent));
        }

        protected async UniTask<GameObject> Instantiate(AssetReference assetReference, Transform parent)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(assetReference);
            return _instantiator.InstantiatePrefab(prefab, parent);
        }

        protected GameObject Instantiate(GameObject prefab)
        {
            return MoveToCurrentScene(_instantiator.InstantiatePrefab(prefab));
        }

        protected GameObject Instantiate(GameObject prefab, Transform parent) =>
            _instantiator.InstantiatePrefab(prefab, parent);

        private GameObject MoveToCurrentScene(GameObject gameObject)
        {
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            return gameObject;
        }
    }
}
