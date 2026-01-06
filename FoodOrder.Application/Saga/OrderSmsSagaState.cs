using MassTransit;
namespace FoodOrder.Application.Saga
{
    public class OrderSmsSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = default!;

        public int OrderId { get; set; }

        public bool FirstSmsSent { get; set; }
        public bool ReadySmsSent { get; set; }
    }
}
