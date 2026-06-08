using Code.Services.AssetProvider;
using Zenject;

namespace Code.Services.Factories.Game
{
    public class GameFactory : Factory, IGameFactory
    {
        public GameFactory(IInstantiator instantiator, IAssetProvider assetProvider) : base(instantiator, assetProvider)
        {
        }
    }
}
