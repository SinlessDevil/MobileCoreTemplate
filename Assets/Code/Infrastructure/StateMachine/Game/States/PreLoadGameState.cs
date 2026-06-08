using Code.Services.PersistenceProgress;
using Code.Services.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Code.Infrastructure.StateMachine.Game.States
{
    public class PreLoadGameState : IPayloadedState<TypeLoad>, IGameState
    {
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly IStaticDataService _staticData;

        public PreLoadGameState(
            IStateMachine<IGameState> stateMachine,
            IPersistenceProgressService persistenceProgressService,
            IStaticDataService staticData)
        {
            _stateMachine = stateMachine;
            _persistenceProgressService = persistenceProgressService;
            _staticData = staticData;
        }

        public UniTaskVoid Enter(TypeLoad payload)
        {
            if (TypeLoad.MenuLoading == payload)
            {
                _stateMachine.Enter<LoadLevelState, string>(_staticData.GameConfig.GameScene);
                return default;
            }

            bool hasCompletedFirstLevel = _persistenceProgressService.PlayerData.PlayerTutorialData.HasFirstCompleteLevel;
            string firstSceneName = FirstSceneName(hasCompletedFirstLevel);

            if (hasCompletedFirstLevel)
                _stateMachine.Enter<LoadMenuState, string>(firstSceneName);
            else
                _stateMachine.Enter<LoadLevelState, string>(firstSceneName);

            return default;
        }

        public UniTaskVoid Exit()
        {
            return default;
        }

        private string FirstSceneName(bool hasCompletedFirstLevel)
        {
            string name = hasCompletedFirstLevel
                ? _staticData.GameConfig.MenuScene
                : _staticData.GameConfig.GameScene;

#if UNITY_EDITOR
            if (_staticData.GameConfig.CanLoadCurrentOpenedScene)
                name = SceneManager.GetActiveScene().name;
#endif
            return name;
        }
    }
}
