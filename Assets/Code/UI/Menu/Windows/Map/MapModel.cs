using System;
using System.Collections.Generic;
using Code.Services.Levels;
using Code.Services.SaveLoad;
using Code.StaticData.Levels;

namespace Code.UI.Menu.Windows.Map
{
    public sealed class MapModel
    {
        private readonly ILevelService _levels;
        private readonly ISaveLoadFacade _saveLoad;
        private IReadOnlyList<ChapterStaticData> _chapters;
        private int _chapterIndex;

        public int ChapterNumber => _chapterIndex + 1;
        public int LevelCount => _chapters.Count == 0 ? 0 : _chapters[_chapterIndex].Levels.Count;
        public bool HasChapters => _chapters.Count > 0;
        public bool CanMoveLeft => _chapterIndex > 0;
        public bool CanMoveRight => _chapterIndex < _chapters.Count - 1;

        public MapModel(ILevelService levels, ISaveLoadFacade saveLoad)
        {
            _levels = levels;
            _saveLoad = saveLoad;
        }

        public void Initialize()
        {
            _chapters = _levels.GetAllChapters();
            _chapterIndex = _chapters.Count == 0 ? 0 : Math.Clamp(_levels.GetCurrentChapterIndex(), 0, _chapters.Count - 1);
        }

        public bool MoveChapter(TypeSwipe direction)
        {
            int offset = direction switch
            {
                TypeSwipe.Left => CanMoveLeft ? -1 : 0,
                TypeSwipe.Right => CanMoveRight ? 1 : 0,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };

            _chapterIndex += offset;
            return offset != 0;
        }

        public bool IsCurrentLevel(int levelNumber) => _levels.IsLevelCurrent(ChapterNumber, levelNumber);
        public bool IsProgressLevel(int levelNumber) => _levels.IsLastCompletedLevel(ChapterNumber, levelNumber);
        public bool IsCompletedLevel(int levelNumber) => _levels.IsLevelCompleted(ChapterNumber, levelNumber);

        public void SelectLevel(int levelNumber)
        {
            _levels.SetUpCurrentLevel(levelNumber, ChapterNumber);
            _saveLoad.SaveProgress(SaveMethod.PlayerPrefs);
        }
    }
}
