using Code.Services.AssetProvider;
using Code.Services.StaticData;
using Code.StaticData;
using Code.UI;
using Code.UI.Game;
using Code.UI.Menu;
using Code.UI.Menu.Windows.Map;
using Code.Window;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Code.Services.Factories.UIFactory
{
    public class UIFactory : Factory, IUIFactory
    {
        private readonly IStaticDataService _staticData;

        private Transform _uiRoot;

        public UIFactory(
            IInstantiator instantiator,
            IAssetProvider assetProvider,
            IStaticDataService staticDataService) : base(instantiator, assetProvider)
        {
            _staticData = staticDataService;
        }

        public GameHud GameHud { get; private set; }
        public MenuHud MenuHud { get; private set; }

        public async UniTask CreateUiRoot()
        {
            _uiRoot = (await Instantiate(ResourcePath.UiRootPath)).transform;
        }

        public RectTransform CrateWindow(WindowTypeId windowTypeId)
        {
            WindowConfig config = _staticData.ForWindow(windowTypeId);
            GameObject window = Instantiate(config.Prefab, _uiRoot);
            return window.GetComponent<RectTransform>();
        }

        public async UniTask<GameHud> CreateGameHud()
        {
            return GameHud = (await Instantiate(ResourcePath.GameHudPath)).GetComponent<GameHud>();
        }

        public async UniTask<MenuHud> CreateMenuHud()
        {
            return MenuHud = (await Instantiate(ResourcePath.MenuHudPath)).GetComponent<MenuHud>();
        }

        public async UniTask<Widget> CreateWidget(Vector3 position, Quaternion rotation)
        {
            return (await Instantiate(ResourcePath.WidgetPath, position, rotation, null)).GetComponent<Widget>();
        }

        public async UniTask<ItemLevel> CreateItemLevel(Transform parent)
        {
            return (await Instantiate(ResourcePath.ItemLevelPath, parent, true)).GetComponent<ItemLevel>();
        }
    }
}
