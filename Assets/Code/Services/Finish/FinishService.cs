using System;
using Code.Services.Finish.Lose;
using Code.Services.Finish.Win;
using Code.Services.Levels;
using Code.StaticData.Levels;

namespace Code.Services.Finish
{
    public class FinishService : IFinishService
    {
        private readonly IWinService _winService;
        private readonly ILoseService _loseService;
        private readonly ILevelService _levelService;

        public FinishService(
            IWinService winService, 
            ILoseService loseService,
            ILevelService levelService)
        {
            _winService = winService;
            _loseService = loseService;
            _levelService = levelService;
        }

        public void Win()
        {
            switch (_levelService.GetCurrentLevelStaticData().LevelTypeId)
            {
                case LevelTypeId.Regular:
                    _winService.Win();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Lose()
        {
            _loseService.Lose();
        }
    }
}
