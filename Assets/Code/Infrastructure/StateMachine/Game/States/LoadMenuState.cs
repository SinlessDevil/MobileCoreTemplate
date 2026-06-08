using Code.Services.AssetProvider;
using Code.Services.Factories.UIFactory;
using Code.Services.PreloaderConductor;
using Code.UI.Menu;
using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.Game.States
{
    public class LoadMenuState : IPayloadedState<string>, IGameState
    {
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IUIFactory _uiFactory;
        private readonly IAssetProvider _assetProvider;
        private readonly IAssetPreloaderConductor _preloaderConductor;

        public LoadMenuState(
            IStateMachine<IGameState> gameStateMachine,
            ISceneLoader sceneLoader,
            ILoadingCurtain loadingCurtain,
            IUIFactory uiFactory,
            IAssetProvider assetProvider,
            IAssetPreloaderConductor preloaderConductor)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _uiFactory = uiFactory;
            _assetProvider = assetProvider;
            _preloaderConductor = preloaderConductor;
        }

        public async UniTaskVoid Enter(string payload)
        {
            _loadingCurtain.Show();
            _assetProvider.CleanUp();
            await _sceneLoader.LoadForce(payload, OnMenuLoad, _loadingCurtain);
        }

        public UniTaskVoid Exit()
        {
            return default;
        }

        private void OnMenuLoad() => SetupMenuWorldAsync().Forget();

        private async UniTask SetupMenuWorldAsync()
        {
            await _uiFactory.CreateUiRoot();
            MenuHud menuHud = await _uiFactory.CreateMenuHud();
            menuHud.Initialize();
            _preloaderConductor.TryPreload();
            _loadingCurtain.Hide();
        }
    }
}
