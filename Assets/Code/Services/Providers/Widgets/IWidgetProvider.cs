using Code.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Services.Providers.Widgets
{
    public interface IWidgetProvider
    {
        UniTask CreatePoolWidgets();
        void CleanupPool();
        UniTask<Widget> GetWidget(Vector3 position, Quaternion rotation);
        void ReturnWidget(Widget widget);
    }
}
