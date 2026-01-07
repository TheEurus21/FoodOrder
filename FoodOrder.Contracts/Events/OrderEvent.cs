namespace FoodOrder.Contracts.Events
{
    public record OrderCreated(Guid CorrelationId,int OrderId, string PhoneNumber, DateTimeOffset ReadyBy);

    public record OrderReady(Guid CorrelationId,int OrderId,string PhoneNumber,DateTimeOffset Ready);
}
