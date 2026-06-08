using Code.UI;
using Code.UI.Game;
using Code.UI.Menu;
using Code.UI.Menu.Windows.Map;
using Code.Window;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Services.Factories.UIFactory
{
    public interface IUIFactory
    {
        GameHud GameHud { get; }
        MenuHud MenuHud { get; }

        UniTask CreateUiRoot();
        UniTask<RectTransform> CreateWindow(WindowTypeId windowTypeId);
        UniTask<GameHud> CreateGameHud();
        UniTask<MenuHud> CreateMenuHud();
        UniTask<Widget> CreateWidget(Vector3 position, Quaternion rotation);
        UniTask<ItemLevel> CreateItemLevel(Transform parent);
    }
}
