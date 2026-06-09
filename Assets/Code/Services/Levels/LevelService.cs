using System.Collections.Generic;
using System.Linq;
using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;
using Code.Services.StaticData;
using Code.Services.Timer;
using Code.StaticData.Levels;

namespace Code.Services.Levels
{
    public class LevelService : ILevelService
    {
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly IStaticDataService _staticDataService;
        private readonly ITimeService _timerService;

        private PlayerLevelData LevelData =>
            _persistenceProgressService.PlayerData.PlayerLevelData;

        public LevelService(
            IPersistenceProgressService persistenceProgressService,
            IStaticDataService staticDataService,
            ITimeService timerService)
        {
            _persistenceProgressService = persistenceProgressService;
            _staticDataService = staticDataService;
            _timerService = timerService;
        }

        public LevelStaticData GetCurrentLevelStaticData() =>
            _staticDataService.ForLevel(LevelData.CurrentProgress.ChapterId, LevelData.CurrentProgress.LevelId);

        public int GetCurrentLevel()   => LevelData.CurrentProgress.LevelId;
        public int GetCurrentChapter() => LevelData.CurrentProgress.ChapterId;

        public int GetCurrentLevelIndex() => LevelData.CurrentProgress.LevelId - 1;

        public int GetCurrentChapterIndex() => LevelData.CurrentProgress.ChapterId - 1;

        public LevelContainer GetCurrentLevelContainer() =>
            LevelData.LevelsComleted.Find(lc =>
                lc.ChapterId == LevelData.CurrentProgress.ChapterId &&
                lc.LevelId   == LevelData.CurrentProgress.LevelId);

        public void SetUpCurrentLevel(int levelNumber, int chapterId)
        {
            LevelData.CurrentProgress.LevelId   = levelNumber;
            LevelData.CurrentProgress.ChapterId = chapterId;
        }

        public void LevelsComplete()
        {
            var current = LevelData.CurrentProgress;

            if (IsLevelCompleted(current.ChapterId, current.LevelId))
                return;

            LevelData.LevelsComleted.Add(new LevelContainer
            {
                ChapterId = current.ChapterId,
                LevelId   = current.LevelId,
                Time      = _timerService.GetElapsedTime()
            });

            AdvanceLastProgress();

            LevelData.CurrentProgress.ChapterId = LevelData.LastProgress.ChapterId;
            LevelData.CurrentProgress.LevelId   = LevelData.LastProgress.LevelId;
        }

        public List<ChapterStaticData> GetAllChapters()
        {
            int lastVirtualChapterId = LevelData.LastProgress.ChapterId;
            var chapters = new List<ChapterStaticData>(lastVirtualChapterId);
            for (int i = 1; i <= lastVirtualChapterId; i++)
                chapters.Add(_staticDataService.ForChapter(i));
            return chapters;
        }

        public bool IsLevelCompleted(int chapterId, int levelId) =>
            LevelData.LevelsComleted.Any(lc =>
                lc.ChapterId == chapterId && lc.LevelId == levelId);

        public bool IsLevelCurrent(int chapterId, int levelId) =>
            LevelData.CurrentProgress.ChapterId == chapterId &&
            LevelData.CurrentProgress.LevelId   == levelId;

        public bool IsLastCompletedLevel(int chapterId, int levelId) =>
            LevelData.LastProgress.ChapterId == chapterId &&
            LevelData.LastProgress.LevelId   == levelId;

        public void Cleanup() { }

        private void AdvanceLastProgress()
        {
            var last    = LevelData.LastProgress;
            var chapter = _staticDataService.ForChapter(last.ChapterId);

            last.LevelId++;

            if (last.LevelId > chapter.Levels.Count)
            {
                last.ChapterId++;
                last.LevelId = 1;
            }
        }
    }
}
