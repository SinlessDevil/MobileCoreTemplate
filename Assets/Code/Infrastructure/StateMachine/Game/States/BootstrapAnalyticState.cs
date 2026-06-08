using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.Game.States
{
    public class BootstrapAnalyticState : IState, IGameState
    {
        private readonly IStateMachine<IGameState> _stateMachine;

        public BootstrapAnalyticState(IStateMachine<IGameState> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public UniTaskVoid Enter()
        {
            _stateMachine.Enter<PreLoadGameState, TypeLoad>(TypeLoad.InitialLoading);
            return default;
        }

        public UniTaskVoid Exit()
        {
            return default;
        }
    }
}
