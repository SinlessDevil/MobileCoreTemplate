using Code.Services.AssetProvider;
using Code.Services.Factories.UIFactory;
using Code.Services.PreloaderConductor;
using Code.Services.Providers.Widgets;
using Code.UI.Game;
using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.Game.States
{
    public class LoadLevelState : IPayloadedState<string>, IGameState
    {
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IUIFactory _uiFactory;
        private readonly IWidgetProvider _widgetProvider;
        private readonly IAssetProvider _assetProvider;
        private readonly IAssetPreloaderConductor _preloaderConductor;

        public LoadLevelState(
            IStateMachine<IGameState> gameStateMachine,
            ISceneLoader sceneLoader,
            ILoadingCurtain loadingCurtain,
            IUIFactory uiFactory,
            IWidgetProvider widgetProvider,
            IAssetProvider assetProvider,
            IAssetPreloaderConductor preloaderConductor)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _uiFactory = uiFactory;
            _widgetProvider = widgetProvider;
            _assetProvider = assetProvider;
            _preloaderConductor = preloaderConductor;
        }

        public async UniTaskVoid Enter(string payload)
        {
            _assetProvider.CleanUp();
            _loadingCurtain.Show();
            await _sceneLoader.LoadForce(payload, OnLevelLoad, _loadingCurtain);
        }

        public UniTaskVoid Exit()
        {
            _loadingCurtain.Hide();
            return default;
        }

        private void OnLevelLoad() => InitGameWorldAsync().Forget();

        private async UniTask InitGameWorldAsync()
        {
            await _uiFactory.CreateUiRoot();
            GameHud gameHud = await _uiFactory.CreateGameHud();
            gameHud.Initialize();
            await _widgetProvider.CreatePoolWidgets();
            _preloaderConductor.TryPreload();
            _gameStateMachine.Enter<GameLoopState>();
        }
    }
}
