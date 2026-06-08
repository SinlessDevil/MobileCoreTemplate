using Code.Services.Factories.UIFactory;
using Code.Window;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Code.Services.Window
{
    public class WindowService : IWindowService
    {
        private IUIFactory _uiFactory;

        [Inject]
        public void Constructor(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public async UniTask<RectTransform> Open(WindowTypeId windowTypeId)
        {
            return await _uiFactory.CreateWindow(windowTypeId);
        }
    }
}