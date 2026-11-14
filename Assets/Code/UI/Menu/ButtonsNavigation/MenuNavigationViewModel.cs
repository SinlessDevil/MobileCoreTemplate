using Code.Services.AudioVibrationFX.Sound;

namespace Code.UI.Menu.ButtonsNavigation
{
    public sealed class MenuNavigationViewModel
    {
        private readonly ISoundService _sound;
        private readonly MenuNavigationModel _model;

        public TypeWindow SelectedWindow => _model.SelectedWindow;

        public MenuNavigationViewModel(ISoundService sound, MenuNavigationModel model)
        {
            _sound = sound;
            _model = model;
        }

        public void Initialize(TypeWindow typeWindow) => _model.SelectedWindow = typeWindow;

        public void SelectWindow(TypeWindow typeWindow)
        {
            _sound.PlaySound(Sound2DType.Click);
            _model.SelectedWindow = typeWindow;
        }
    }

    public sealed class MenuNavigationModel
    {
        public TypeWindow SelectedWindow { get; set; }
    }
}
