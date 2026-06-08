using System.Collections.Generic;
using System.Linq;
using Code.Services.Factories.UIFactory;
using Code.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Services.Providers.Widgets
{
    public class WidgetProvider : IWidgetProvider
    {
        private const int InitialCount = 10;

        private List<Widget> _pool;

        private readonly IUIFactory _uiFactory;

        public WidgetProvider(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public async UniTask CreatePoolWidgets()
        {
            _pool = new List<Widget>();

            for (int i = 0; i < InitialCount; i++)
            {
                Widget widget = await CreateObject(Vector3.zero, Quaternion.identity);
                widget.Deactivate();
            }
        }

        public void CleanupPool()
        {
            foreach (Widget widget in _pool.Where(w => w != null))
                Object.Destroy(widget.gameObject);

            _pool.Clear();
        }

        public async UniTask<Widget> GetWidget(Vector3 position, Quaternion rotation)
        {
            foreach (Widget widget in _pool)
            {
                if (!widget.gameObject.activeInHierarchy)
                {
                    widget.Activate(position, rotation);
                    return widget;
                }
            }

            Widget newWidget = await CreateObject(position, rotation);
            newWidget.Activate(position, rotation);
            return newWidget;
        }

        public void ReturnWidget(Widget widget)
        {
            widget.Deactivate();
        }

        private async UniTask<Widget> CreateObject(Vector3 position, Quaternion rotation)
        {
            Widget widget = await _uiFactory.CreateWidget(position, rotation);
            _pool.Add(widget);
            return widget;
        }
    }
}
