using Code.Services.AudioVibrationFX.Sound;
using Code.Services.Timer;
using Code.Services.Window;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Window.Setting
{
    public class ButtonSetting : MonoBehaviour
    {
        [SerializeField] private Button _button;
        
        private SettingWindow _settingWindow;

        private ITimeService _timeService;
        private ISoundService _soundService;
        private IWindowService _windowService;
        
        [Inject]
        public void Constructor(
            ITimeService timeService,
            ISoundService soundService,
            IWindowService windowService)
        {
            _timeService = timeService;
            _soundService = soundService;
            _windowService = windowService;
        }
        
        private void OnEnable() => _button.onClick.AddListener(OpenPauseWindow);
        private void OnDisable() => _button.onClick.RemoveListener(OpenPauseWindow);

        private void OpenPauseWindow()
        {
            _soundService.PlaySound(Sound2DType.Click);
            _timeService.Pause();
            OpenPauseWindowAsync().Forget();
        }

        private async UniTaskVoid OpenPauseWindowAsync()
        {
            var prefab = await _windowService.Open(WindowTypeId.Setting);
            _settingWindow = prefab.GetComponent<SettingWindow>();
            _settingWindow.UpdateWindow();
            _settingWindow.ResetButtonScale();
        }
    }
}