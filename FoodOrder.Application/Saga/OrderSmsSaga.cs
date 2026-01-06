using FoodOrder.Contracts.Events;
using FoodOrder.Contracts.Commands; 
using MassTransit;

namespace FoodOrder.Application.Saga
{
    public class OrderSmsSaga : MassTransitStateMachine<OrderSmsSagaState>
    {
        public State WaitingForReady { get; private set; } = default!;

        public Event<OrderCreated> OrderCreated { get; private set; } = default!;
        public Event<OrderReady> OrderReady { get; private set; } = default!;

        public OrderSmsSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreated, x =>
            {
                x.CorrelateById(ctx => CreateCorrelationId(ctx.Message.OrderId));
                x.InsertOnInitial = true;
            });

            Event(() => OrderReady, x =>
            {
                x.CorrelateById(ctx => CreateCorrelationId(ctx.Message.OrderId));
            });

            Initially(
                When(OrderCreated)
                    .Then(ctx =>
                    {
                        ctx.Saga.OrderId = ctx.Message.OrderId;
                    })
                    .Publish(ctx => new SendSms(
                        ctx.Message.PhoneNumber,
                        $"Your order will be ready by {ctx.Message.ReadyBy:t}"
                    ))
                    .Then(ctx => ctx.Saga.FirstSmsSent = true)
                    .TransitionTo(WaitingForReady)
            );

            During(WaitingForReady,
                When(OrderReady)
                    .Publish(ctx => new SendSms(
                        ctx.Message.PhoneNumber,
                        $"Your order is ready. Come pick it up!"
                    ))
                    .Then(ctx => ctx.Saga.ReadySmsSent = true)
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }

        static Guid CreateCorrelationId(int orderId)
     => Guid.NewGuid();

    }
}