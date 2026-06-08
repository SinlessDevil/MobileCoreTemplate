using System;

namespace Code.Infrastructure
{
    public interface ILoadingCurtain
    {
        bool IsActive { get; }
        void Show();
        void Hide();
        void ShowProgress(float progress);
        void ShowNoInternetWarning(Action onContinueClick);
    }
}