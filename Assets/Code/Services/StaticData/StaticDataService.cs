using System;
using System.Collections.Generic;
using System.Linq;
using Code.Services.Factories;
using Code.StaticData;
using Code.StaticData.Levels;
using Code.Window;
using UnityEngine;

namespace Code.Services.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private GameStaticData _gameStaticData;
        private BalanceStaticData _balanceStaticData;
        private PreloadConfig _preloadConfig;
        private Dictionary<WindowTypeId, WindowConfig> _windowConfigs;
        private List<ChapterStaticData> _chapterStaticDatas = new();

        public GameStaticData GameConfig => _gameStaticData;
        public BalanceStaticData Balance => _balanceStaticData;
        public List<ChapterStaticData> Chapters => _chapterStaticDatas;
        public PreloadConfig PreloadConfig => _preloadConfig;
        
        public void LoadData()
        {
            _gameStaticData = Resources
                .Load<GameStaticData>(ResourcePath.GameConfigPath);

            _balanceStaticData = Resources
                .Load<BalanceStaticData>(ResourcePath.GameBalancePath);

            _windowConfigs = Resources
                .Load<WindowStaticData>(ResourcePath.WindowsStaticDataPath)
                .Configs.ToDictionary(x => x.WindowTypeId, x => x);

            _chapterStaticDatas = Resources
                .LoadAll<ChapterStaticData>(ResourcePath.ChaptersStaticDataPath)
                .ToList();

            _preloadConfig = Resources
                .Load<PreloadConfig>(ResourcePath.PreloadConfigPath);
        }

        public WindowConfig ForWindow(WindowTypeId windowTypeId) => _windowConfigs[windowTypeId];

        public LevelStaticData ForLevel(int chapterId, int levelId)
        {
            if (_chapterStaticDatas.Count == 0)
                throw new InvalidOperationException("No chapters available.");

            int realChapterIndex = (chapterId - 1) % _chapterStaticDatas.Count;
            var chapter = _chapterStaticDatas[realChapterIndex];

            if (chapter.Levels.Count == 0)
                throw new InvalidOperationException($"Chapter {realChapterIndex + 1} has no levels.");

            if (levelId < 1 || levelId > chapter.Levels.Count)
                throw new ArgumentOutOfRangeException(nameof(levelId), $"Level {levelId} is out of range: 1 - {chapter.Levels.Count}");

            var level = chapter.Levels[levelId - 1];
            return level;
        }
        
        public ChapterStaticData ForChapter(int chapterId)
        {
            if (_chapterStaticDatas.Count == 0)
                throw new InvalidOperationException("No chapters available.");

            int realChapterIndex = (chapterId - 1) % _chapterStaticDatas.Count;

            return _chapterStaticDatas[realChapterIndex];
        }
    }
}