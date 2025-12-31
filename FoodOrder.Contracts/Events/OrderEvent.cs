namespace FoodOrder.Contracts.Events
{
    public record OrderCreated(int OrderId, string PhoneNumber, DateTimeOffset ReadyBy);

    public record OrderReady(int OrderId,string PhoneNumber,DateTimeOffset Ready);
}
