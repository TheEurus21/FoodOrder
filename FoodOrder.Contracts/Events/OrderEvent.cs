namespace FoodOrder.Contracts.Events
{
    public record OrderCreated(int OrderId, string PhoneNumber, DateTime ReadyBy);

    public record OrderReady(Guid OrderId);
}
