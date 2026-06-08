using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.Game.States
{
    public interface IState : IExitable
    {
        UniTaskVoid Enter();
    }
}