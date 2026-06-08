using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;
using Code.Services.SaveLoad;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Infrastructure.StateMachine.Game.States
{
    public class LoadProgressState : IState, IGameState
    {
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly IPersistenceProgressService _progressService;
        private readonly ISaveLoadFacade _saveLoadFacade;

        public LoadProgressState(
            IStateMachine<IGameState> stateMachine,
            IPersistenceProgressService progressService,
            ISaveLoadFacade saveLoadFacade)
        {
            _stateMachine = stateMachine;
            _progressService = progressService;
            _saveLoadFacade = saveLoadFacade;
        }

        public UniTaskVoid Enter()
        {
            LoadOrCreatePlayerData();
            InitLoadingVersion();
            _stateMachine.Enter<BootstrapAnalyticState>();
            return default;
        }

        public UniTaskVoid Exit()
        {
            return default;
        }

        private void LoadOrCreatePlayerData()
        {
            _progressService.PlayerData =
                _saveLoadFacade.Load(SaveMethod.PlayerPrefs) ?? new PlayerData();
        }

        private void InitLoadingVersion()
        {
            string version = Application.version;
            if (_progressService.PlayerData.Loading.Version != version)
                _progressService.PlayerData.Loading.Reset(version);
        }
    }
}
