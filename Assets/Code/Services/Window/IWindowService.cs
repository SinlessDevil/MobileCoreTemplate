using Code.Window;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Services.Window
{
    public interface IWindowService
    {
        UniTask<RectTransform> Open(WindowTypeId windowTypeId);
    }
}