namespace FoodOrder.Contracts.Commands
{
 
        public record SendSms(string PhoneNumber, string Message);
    
}
