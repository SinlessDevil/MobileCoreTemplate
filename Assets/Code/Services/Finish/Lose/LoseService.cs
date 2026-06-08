using Code.Services.Window;
using Code.Window;
using Code.Window.Finish.Lose;
using Cysharp.Threading.Tasks;

namespace Code.Services.Finish.Lose
{
    public class LoseService : ILoseService
    {
        private IWindowService _windowService;

        public LoseService(
            IWindowService windowService)
        {
            _windowService = windowService;
        }

        public void Lose() => ShowLoseWindowAsync().Forget();

        private async UniTaskVoid ShowLoseWindowAsync()
        {
            var window = await _windowService.Open(WindowTypeId.Lose);
            var loseWindow = window.GetComponent<LoseWindow>();
            loseWindow.Initialize();
        }
    }
}