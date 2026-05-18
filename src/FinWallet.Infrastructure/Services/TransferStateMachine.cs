using FinWallet.Application.Transactions.Events;
using MassTransit;

namespace FinWallet.Infrastructure.Services;

public sealed class TransferStateMachine : MassTransitStateMachine<TransferSagaState>
{
    public State Submitted { get; private set; } = null!;
    public Event<TransferRequestedIntegrationEvent> TransferRequested { get; private set; } = null!;
    public Event<TransferCompletedIntegrationEvent> TransferCompleted { get; private set; } = null!;
    public Event<TransferFailedIntegrationEvent> TransferFailed { get; private set; } = null!;

    public TransferStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TransferRequested, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => TransferCompleted, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => TransferFailed, x => x.CorrelateById(context => context.Message.TransferId));

        Initially(
            When(TransferRequested)
                .Then(context =>
                {
                    context.Saga.FromWalletId = context.Message.FromWalletId;
                    context.Saga.ToWalletId = context.Message.ToWalletId;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.Reference = context.Message.Reference;
                    context.Saga.CreatedAtUtc = context.Message.CreatedAtUtc;
                })
                // Ghi chu: Saga don gian chi ghi nhan va phat su kien hoan tat.
                .Publish(context => new TransferCompletedIntegrationEvent(
                    context.Message.TransferId,
                    DateTime.UtcNow))
                .TransitionTo(Submitted));

        During(Submitted,
            When(TransferCompleted)
                .Finalize(),
            When(TransferFailed)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
