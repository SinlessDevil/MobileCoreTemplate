using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.Game.States
{
    public interface IPayloadedState<TPayload> : IExitable
    {
        UniTaskVoid Enter(TPayload payload);
    }
}