using MassTransit;

public class OrderSmsSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string ?CurrentState { get; set; } 
    public bool FirstSmsSent { get; set; }
    public bool ReadySmsSent { get; set; }
}
