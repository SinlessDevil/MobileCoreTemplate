using System;
using Cysharp.Threading.Tasks;

namespace Code.Infrastructure
{
    public interface ISceneLoader
    {
        UniTask LoadForce(string name, Action onLevelLoad, ILoadingCurtain loadingCurtain);
        UniTask Load(string name, Action onLevelLoad, bool isAddressable, ILoadingCurtain loadingCurtain);
    }
}