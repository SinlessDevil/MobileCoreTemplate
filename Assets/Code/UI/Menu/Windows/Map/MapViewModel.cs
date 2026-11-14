using System.Collections.Generic;
using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Game.States;
using Code.Services.AudioVibrationFX.Sound;
using Code.Services.StaticData;

namespace Code.UI.Menu.Windows.Map
{
    public sealed class MapViewModel
    {
        private readonly MapModel _model;
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly IStaticDataService _staticData;
        private readonly ISoundService _sound;

        public MapViewModel(MapModel model, IStateMachine<IGameState> stateMachine, IStaticDataService staticData,
            ISoundService sound)
        {
            _model = model;
            _stateMachine = stateMachine;
            _staticData = staticData;
            _sound = sound;
        }

        public MapViewData Initialize()
        {
            _model.Initialize();
            return GetViewData();
        }

        public bool TryMoveChapter(TypeSwipe direction, out MapViewData data)
        {
            if (!_model.MoveChapter(direction))
            {
                data = default;
                return false;
            }

            _sound.PlaySound(Sound2DType.Click);
            data = GetViewData();
            return true;
        }

        public void StartLevel(int levelNumber, int chapterNumber)
        {
            if (chapterNumber != _model.ChapterNumber || levelNumber < 1 || levelNumber > _model.LevelCount ||
                GetLevelStatus(levelNumber) == LevelViewStatus.Locked)
                return;

            _sound.PlaySound(Sound2DType.Click);
            _model.SelectLevel(levelNumber);
            _stateMachine.Enter<LoadLevelState, string>(_staticData.GameConfig.GameScene);
        }

        private MapViewData GetViewData()
        {
            int chapterNumber = _model.ChapterNumber;
            LevelViewData[] levels = new LevelViewData[_model.LevelCount];
            for (int index = 0; index < levels.Length; index++)
            {
                int levelNumber = index + 1;
                levels[index] = new LevelViewData(levelNumber, chapterNumber, levelNumber.ToString(), GetLevelStatus(levelNumber));
            }

            string title = _model.HasChapters ? $"Chapter {chapterNumber}" : string.Empty;
            return new MapViewData(title, _model.CanMoveLeft, _model.CanMoveRight, levels);
        }

        private LevelViewStatus GetLevelStatus(int levelNumber)
        {
            if (_model.IsCurrentLevel(levelNumber))
                return LevelViewStatus.Current;
            if (_model.IsProgressLevel(levelNumber))
                return LevelViewStatus.Unlocked;

            return _model.IsCompletedLevel(levelNumber) ? LevelViewStatus.Completed : LevelViewStatus.Locked;
        }
    }

    public readonly struct MapViewData
    {
        public string ChapterTitle { get; }
        public bool CanSwipeLeft { get; }
        public bool CanSwipeRight { get; }
        public IReadOnlyList<LevelViewData> Levels { get; }

        public MapViewData(string chapterTitle, bool canSwipeLeft, bool canSwipeRight,
            IReadOnlyList<LevelViewData> levels)
        {
            ChapterTitle = chapterTitle;
            CanSwipeLeft = canSwipeLeft;
            CanSwipeRight = canSwipeRight;
            Levels = levels;
        }
    }

    public readonly struct LevelViewData
    {
        public int LevelNumber { get; }
        public int ChapterNumber { get; }
        public string Label { get; }
        public LevelViewStatus Status { get; }

        public LevelViewData(int levelNumber, int chapterNumber, string label, LevelViewStatus status)
        {
            LevelNumber = levelNumber;
            ChapterNumber = chapterNumber;
            Label = label;
            Status = status;
        }
    }

    public enum LevelViewStatus
    {
        Locked,
        Unlocked,
        Current,
        Completed
    }
}
